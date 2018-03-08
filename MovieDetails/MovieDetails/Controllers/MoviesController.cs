using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MovieDetails.Entity;
using MovieDetails.Models;
using System.IO;

namespace MovieDetails.Controllers
{
    public class MoviesController : Controller
    {
        private MOVIEDBEntities db = new MOVIEDBEntities();

        // GET: Movies
        public ActionResult Index()
        {
            var actors = db.Actors.Select(x => x.Name);
            var producers = db.Producers.Select(x => x.Name);

             

            List<MoviesDto> movies = new List<MoviesDto>();
            foreach (var item in db.Movies.ToList())
            {
                movies.Add(new MoviesDto
                {
                    Id = item.Id,
                    Name = item.Name,                    
                    ReleasedYear = item.ReleasedYear,
                    Producer = item.MovieProducerTables.FirstOrDefault(x => x.MovieId == item.Id).Producer.Name,
                    Actors = item.MovieActorTables.Where(x => x.MovieId == item.Id).Select(x => x.Actor.Name).ToList(),
                    ProducerList =
                  new SelectList(producers
                  .Select(x => new { value = x, text = x }),
                  "value", "text", item.MovieProducerTables.FirstOrDefault(x => x.MovieId == item.Id).Producer.Name),
                    ActorList = new List<CheckList>()
            });
            }           
            
            
            return View(movies);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(IEnumerable<MoviesDto> movies)
        {
            var producers = db.Producers.Select(x => x.Name);
           
                foreach (var item in movies)
                {
                    Movie movie = db.Movies.FirstOrDefault(x => x.Id == item.Id);
                    movie.Name = item.Name;
                    movie.ReleasedYear = item.ReleasedYear;
                    db.Entry(movie).State = EntityState.Modified;
                    var producerId = db.Producers.FirstOrDefault(x => x.Name == item.Producer).Id;
                    MovieProducerTable movieproducer = db.MovieProducerTables.FirstOrDefault(x => x.MovieId == item.Id);
                    movieproducer.ProducerId = producerId;
                    db.Entry(movieproducer).State = EntityState.Modified;
                }
              
                   
                
           db.SaveChanges();
               
                //return RedirectToAction("Index");
            
            for (int i = 0; i < db.Movies.ToList().Count; i++)
            {
                movies.ToList()[i].Actors = db.Movies.ToList()[i].MovieActorTables.Where(x => x.MovieId == db.Movies.ToList()[i].Id).Select(x => x.Actor.Name).ToList();
                movies.ToList()[i].ProducerList = new SelectList(producers
               .Select(x => new { value = x, text = x }),
               "value", "text", db.Movies.ToList()[i].MovieProducerTables.FirstOrDefault(x => x.MovieId == db.Movies.ToList()[i].Id).Producer.Name);
            }
            return View(movies);
        }

        // GET: Movies/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Movie movie = db.Movies.Find(id);
            if (movie == null)
            {
                return HttpNotFound();
            }
            return View(movie);
        }

        // GET: Movies/Create
        public ActionResult Create()
        {
            var actors = db.Actors.Select(x => x.Name);
            var producers = db.Producers.Select(x => x.Name);

            MoviesDto movieModel = new MoviesDto();            
            movieModel.ProducerList =
                    new SelectList(producers
                    .Select(x => new { value = x, text = x }),
                    "value", "text");

            movieModel.ActorList = new List<CheckList>();
            foreach(var item in actors)
            {
                movieModel.ActorList.Add(new CheckList
                {
                    Name = item,
                    isChecked = false
                });
            }
            
            return View(movieModel);
        }



        public ActionResult CreateProducer(string producer)
        {
            var actors = db.Actors.Select(x => x.Name);
            var producers = db.Producers.Select(x => x.Name);
            ViewBag.Producers = producers;
            ViewBag.Actors = actors;
            return View("Create");
        }

        // POST: Movies/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(MoviesDto movie, string command)
        {
            if (command == "CreateProducer")
            {
                
                    Producer newProducer = new Producer
                    {
                        Name = movie.PersonName,
                        Sex = movie.Sex.ToString(),
                        DOB = movie.DOB,
                        Bio = movie.Bio
                    };
                    db.Producers.Add(newProducer);
                    db.SaveChanges();
                
                
                var actors = db.Actors.Select(x => x.Name);
                var producers = db.Producers.Select(x => x.Name);

                MoviesDto newMovie = new MoviesDto();

                
                
                movie.ProducerList =
                    new SelectList(producers
                    .Select(x => new { value = x, text = x }),
                    "value", "text", movie.PersonName);
                movie.PersonName = "";
                movie.Bio = "";
                movie.DOB = default(DateTime);
                ViewBag.Actors = actors;
                return View(movie);

            }
            if (command == "CreateActor")
            {
                var producers = db.Producers.Select(x => x.Name);
                var actors = db.Actors.Select(x => x.Name);
                movie.ActorList = new List<CheckList>();
                foreach (var item in actors)
                {
                    movie.ActorList.Add(new CheckList
                    {
                        Name = item,
                        isChecked = false
                    });
                }
                Actor newActor = new Actor
                {
                    Name = movie.PersonName,
                    Sex = movie.Sex.ToString(),
                    DOB = movie.DOB,
                    Bio = movie.Bio
                };
                db.Actors.Add(newActor);
                db.SaveChanges();                                           
                movie.ActorList.Add(new CheckList
                {
                    Name = movie.PersonName,
                    isChecked = true
                });
                movie.ProducerList =
                    new SelectList(producers
                    .Select(x => new { value = x, text = x }),
                    "value", "text");               
                ViewBag.Actors = actors;
                return View(movie);

            }
            if (ModelState.IsValid && command == "CreateMovie")
            {
                Movie newMovie = new Movie
                {
                    Name = movie.Name,
                    ReleasedYear = movie.ReleasedYear,
                    Plot = movie.Plot                   
                };


                db.Movies.Add(newMovie);
                db.SaveChanges();

                MovieProducerTable newMovieProducerTableData = new MovieProducerTable
                {
                    MovieId = db.Movies.FirstOrDefault(x => x.Name == movie.Name).Id,
                    ProducerId = db.Producers.FirstOrDefault(x => x.Name == movie.Producer).Id
                };

                List<MovieActorTable> newMovieActorTableData = new List<MovieActorTable>();

                foreach(var item in movie.ActorList.Where(x=>x.isChecked == true))
                {
                    newMovieActorTableData.Add(new MovieActorTable
                    {
                        MovieId = db.Movies.FirstOrDefault(x => x.Name == movie.Name).Id,
                        ActorId = db.Actors.FirstOrDefault(x => x.Name == item.Name).Id
                    });
                }
                

                db.MovieProducerTables.Add(newMovieProducerTableData);
                db.MovieActorTables.AddRange(newMovieActorTableData);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(movie);
        }

        // GET: Movies/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Movie movie = db.Movies.Find(id);
            if (movie == null)
            {
                return HttpNotFound();
            }
            return View(movie);
        }

        // POST: Movies/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,ReleasedYear,Plot,Poster")] Movie movie)
        {
            if (ModelState.IsValid)
            {
                db.Entry(movie).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(movie);
        }

        // GET: Movies/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Movie movie = db.Movies.Find(id);
            if (movie == null)
            {
                return HttpNotFound();
            }
            return View(movie);
        }

        // POST: Movies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Movie movie = db.Movies.Find(id);
            db.Movies.Remove(movie);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
