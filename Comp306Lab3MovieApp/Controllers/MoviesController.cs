using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Comp306Lab3MovieApp.Data;
using Comp306Lab3MovieApp.Models;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.S3.Util;
using Amazon.S3.Model;
using Amazon.S3;
using System.IO;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Amazon.S3.Transfer;

namespace Comp306Lab3MovieApp.Controllers
{
    public class MoviesController : Controller
    {
        private AmazonDynamoDBClient client;
        private IAmazonS3 amazonS3;
        private DynamoDBContext _dbContext;
        private readonly MovieAppContext _context;
        List<Review> reviewList;
        string BUCKET_NAME = "comp306-movieweb-lab3";
        Movie updateMovie;

        public MoviesController(MovieAppContext context)
        {
            _context = context;
            client = new AmazonDynamoDBClient(RegionEndpoint.USEast2);
            _dbContext = new DynamoDBContext(client);
            amazonS3 = new AmazonS3Client(RegionEndpoint.USEast2);
            reviewList = new List<Review>();

        }

        // GET: Movies
        public async Task<IActionResult> Index()
        {
            return View(await _context.Movies.ToListAsync());
        }

        public async Task<IActionResult> Search(string mName)
        {
            var movie = await _context.Movies
              .FirstOrDefaultAsync(m => m.MovieName == mName);
            if (movie == null)
            {
                return NotFound();
            }
            return Redirect("Details");
        }

        // GET: Movies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            int movieId = 0;
            if (id == null)
            {
                return NotFound();
            }
            if (TempData["RevewMovieId"] != null)
            {
                movieId = (int)TempData["ReviewMovieId"];
            }

            var movie = await _context.Movies
                .FirstOrDefaultAsync(m => m.MovieId == id || m.MovieId == movieId);

            if (movie == null)
            {
                return NotFound();
            }
            await GetReviewsList(movie);
            return View(movie);
        }

        [HttpGet]
        // GET: Movies/Create
        public IActionResult Create()
        {
            return View(new Movie());
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Movie movie)
        {
            if (ModelState.IsValid)
            {
                int id = (int)TempData["UserId"];
                User user = _context.Users.FirstOrDefault(u => u.UserId == id);
                movie.User = user;
                _context.Add(movie);
                await _context.SaveChangesAsync();
                TempData["Created"] = "Movie added successfull!";
                return RedirectToAction(nameof(Index));
            }
            return View(movie);
        }

        // GET: Movies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies.FindAsync(id);
            if (movie == null)
            {
                return NotFound();
            }
            return View(movie);
        }

        // POST: Movies/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Movie movie)
        {

            if (id != movie.MovieId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    updateMovie = new Movie
                    {
                        MovieId = movie.MovieId,
                        MovieName = movie.MovieName,
                        Description = movie.Description,
                        FilePath = movie.FilePath,
                        Genre = movie.Genre,
                        ImageUrl = movie.ImageUrl,
                        Rating = movie.Rating,
                        ReleaseDate = movie.ReleaseDate,
                        User = movie.User
                    };
                    _context.Update(updateMovie);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MovieExists(movie.MovieId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["Updated"] = "Movie is updated successfully!";
                return RedirectToAction("Index");
            }
            return View(movie);
        }

        // GET: Movies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies
                .FirstOrDefaultAsync(m => m.MovieId == id);
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // POST: Movies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            _context.Movies.Remove(movie);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MovieExists(int id)
        {
            return _context.Movies.Any(e => e.MovieId == id);
        }

        public async Task GetReviewsList(Movie movie)
        {
            string tabName = "Reviews";
            double calculateAvgRate = 0;
            int sumRate = 0;
            var currentTables = await client.ListTablesAsync();
            if (currentTables.TableNames.Contains(tabName))
            {
                IEnumerable<Review> movieReviews;
                var conditions = new List<ScanCondition> { new ScanCondition("MovieId", ScanOperator.Equal, movie.MovieId) };
                movieReviews = await _dbContext.ScanAsync<Review>(conditions).GetRemainingAsync();

                Console.WriteLine("List retrieved " + movieReviews);
                foreach (var result in movieReviews)
                {
                    if (result != null)
                    {
                        Review review = new Review()
                        {
                            ReviewDescription = result.ReviewDescription,
                            MovieRating = result.MovieRating,
                            Title = result.Title,
                            MovieId = result.MovieId,
                            ReviewID = result.ReviewID,
                            UserEmail = result.UserEmail,
                        };
                        reviewList.Add(review);
                        sumRate += result.MovieRating;
                        calculateAvgRate++;
                    }

                }
                movie.Rating = sumRate / calculateAvgRate;
            }
        }


        [HttpGet]
        public ActionResult AddMovieFile(int id)
        {

            return View(id);
        }

        [HttpPost]
        public async Task<IActionResult> AddMovieFile(IFormFile postedFile, int id)
        {
            Movie movie = _context.Movies.FirstOrDefault(m => m.MovieId == id);
            var fileTransferUtility = new TransferUtility(amazonS3);
            string filePath = postedFile.FileName.ToString();
            try
            {
                if (postedFile.Length > 0)
                {
                    var tempPath = Path.GetTempFileName();

                    using (var stream = new FileStream(tempPath, FileMode.Create))
                    {
                        await postedFile.CopyToAsync(stream);
                    }
                    string keyName = postedFile.FileName;
                    filePath = tempPath;
                    var fileTransferUtilityRequest = new TransferUtilityUploadRequest
                    {
                        BucketName = BUCKET_NAME,
                        FilePath = filePath,
                        StorageClass = S3StorageClass.StandardInfrequentAccess,
                        PartSize = 6291456, // 6 MB.
                        Key = keyName,
                        CannedACL = S3CannedACL.PublicRead
                    };
                    fileTransferUtilityRequest.Metadata.Add("MovieId", movie.MovieId.ToString());
                    await fileTransferUtility.UploadAsync(fileTransferUtilityRequest);

                    movie.FilePath = keyName;
                    _context.Update(movie);
                    await _context.SaveChangesAsync();
                    TempData["MovieUploaded"] = $"{movie.MovieName} is successfully uploaded!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine("Error occured. Message:'{0}' ", e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unknown error occured. Message: '{0}'", e.Message);
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> DownloadMovie(int id)
        {
            Movie movie = await _context.Movies.FindAsync(id);
            if (movie.FilePath != null)
            {
                try
                {
                    using (amazonS3)
                    {
                        string keyName = movie.FilePath;
                        GetPreSignedUrlRequest request =
                                  new GetPreSignedUrlRequest()
                                  {
                                      BucketName = BUCKET_NAME,
                                      Key = keyName,
                                      Expires = DateTime.Now.AddMinutes(15)
                                  };

                        string url = amazonS3.GetPreSignedURL(request);
                        return Redirect(url);
                    }
                }
                catch (Exception)
                {
                    string Failure = "File download failed. Please try after some time.";
                    return View(Failure);
                }
            }
            else
            {
                TempData["NoFile"] = "No file exist to download";
                return RedirectToAction("Index");
            }



        }

        public async Task ListingObjectsAsync(string filePath)
        {
            try
            {
                ListObjectsV2Request request = new ListObjectsV2Request
                {
                    BucketName = BUCKET_NAME,
                    MaxKeys = 10
                };
                ListObjectsV2Response response;
                do
                {
                    response = await amazonS3.ListObjectsV2Async(request);

                    // Process the response.
                    foreach (S3Object entry in response.S3Objects)
                    {
                        if (entry.Key == filePath)
                        {

                        }
                        Console.WriteLine("key = {0} size = {1}",
                            entry.Key, entry.Size);
                    }
                    Console.WriteLine("Next Continuation Token: {0}", response.NextContinuationToken);
                    request.ContinuationToken = response.NextContinuationToken;
                } while (response.IsTruncated);
            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                Console.WriteLine("S3 error occurred. Exception: " + amazonS3Exception.ToString());
                Console.ReadKey();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.ToString());
                Console.ReadKey();
            }
        }


    }
}
