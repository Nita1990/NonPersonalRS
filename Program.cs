using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;

namespace RSDZ1
{
    class UserRating
    {
        public int User;
        public int Rating;

        public UserRating( int user, int rating)
        {
            User = user;
            Rating = rating;
        }
    }

    class Film
    {
        public string Id;
        public double AverageScore;

        public Film( string id)
        {
            Id = id;
        }
    }

    class Program
    {
       
        static void Main(string[] args)
        {
            Dictionary<string, List<int>> ratings = new Dictionary<string, List<int>>();
            List<Film> films = new List<Film>();

            #region считывание данных из файла
            string raitingsFilename = "ratings.csv";
            int i = 0;

            using (StreamReader stream = new StreamReader(raitingsFilename))
            {
                bool first = true;
                while (!stream.EndOfStream)
                {
                    string[] curRow = stream.ReadLine().Split('|');
                    if (!first) /*строка заголовка не интересует*/
                    {
                   
                        string userId = curRow[0];
                        string filmId = curRow[1];

                        if (curRow[2] == "" )
                            continue;

                        int rating = Convert.ToInt32(curRow[2]);

                        if (!ratings.ContainsKey(filmId))
                        {

                            List<int> filmRatings = new List<int>();
                            ratings.Add(filmId, filmRatings);
                            films.Add( new Film( filmId ));
                        }

                        List<int> curFilmRatings = ratings[filmId];
                        curFilmRatings.Add(rating);
                           
                        i++;
                    }
                    else
                        first = false;

                }
            }

            #endregion

            FindAverageScore(films, ratings);
            Console.ReadLine();

        }

        /// <summary>
        /// алгоритм average
        /// </summary>
        /// <param name="allFilms"></param>
        /// <param name="?"></param>
        /// <param name="ratings"></param>
        public static void FindAverageScore( List<Film> allFilms,  Dictionary<string, List<int>> ratings )
        {
           /* для каждого фильма находим среднее значение*/
            foreach (Film film in allFilms)
            {
                List<int> filmRatings = ratings[film.Id];

                int totalScore = 0;
                foreach (int oneRating in filmRatings)
                    totalScore += oneRating;

                film.AverageScore = Convert.ToDouble(totalScore) / filmRatings.Count;
            }

            List<Film>list = allFilms.OrderByDescending(film => film.AverageScore).ToList<Film>();
            for (int i = 0; i <= 9; i++)
            { 
                Console.WriteLine( "filmId: "  + list[i].Id +  " score "  + Math.Round( list[i].AverageScore, 2) );
            }
        }
    }
}
