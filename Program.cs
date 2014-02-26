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
        public double Score;
        public List<int> Ratings = new List<int>();

        public Film( string id)
        {
            Id = id;
        }
    }

    class Program
    {
       
        static void Main(string[] args)
        {
            #region Получение параметров из командной строки
            string ratingsFilename = GetParamFromArgs("-r", args);
            if (ratingsFilename == null)
                return;

            string titlesFilename = GetParamFromArgs("-n", args);
            if (titlesFilename == null)
                return;

            string methodName = GetParamFromArgs("-m", args);
            if (methodName == null)
                return;

            string filmIdForAssociation;
            if (methodName == "association")
            {
                filmIdForAssociation = GetParamFromArgs("-x", args);
                if( filmIdForAssociation == null )
                    return;
            }

            string outputFilename = GetParamFromArgs("-o", args);
            if( outputFilename == null )
                return;
            #endregion

            Dictionary<string, Film> filmsDic = new Dictionary<string,Film>();
            
            #region считывание данных из файла
            int i = 0;

            int strange = 0;
            using (StreamReader stream = new StreamReader(ratingsFilename))
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
                        {
                            strange++;
                            continue;
                        }

                        int rating = Convert.ToInt32(curRow[2]);

                        if (!filmsDic.ContainsKey(filmId))
                        {

                            Film film = new Film( filmId );
                            filmsDic.Add(filmId, film);
                        }

                        List<int> curFilmRatings = filmsDic[filmId].Ratings;
                        curFilmRatings.Add(rating);
                           
                        i++;
                    }
                    else
                        first = false;

                }
            }

            #endregion

            Console.WriteLine( "str " + strange.ToString());
            switch( methodName )
            {
                case "average":
                    FindAgregatedOpinion(filmsDic, outputFilename, "average");
                    break;
                case "netvotes":
                    FindAgregatedOpinion(filmsDic, outputFilename, "netvotes");
                    break;
                case "positive":
                    FindAgregatedOpinion(filmsDic, outputFilename, "positive");
                    break;
                default:
                    Console.WriteLine("Задан неверный тип метода");
                    break;
            }
            Console.ReadLine();

        }

        private static string GetParamFromArgs(string paramName, string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == paramName)
                    return args[i + 1];
            }

            Console.WriteLine("Не задан параметр " + paramName);
            return null;
        }

        /// <summary>
        /// алгоритм average
        /// </summary>
        /// <param name="allFilms"></param>
        /// <param name="?"></param>
        /// <param name="ratings"></param>
        public static void FindAgregatedOpinion(Dictionary<string, Film> filmsDic, string outputFilename, string method)
        {
           /* для каждого фильма находим среднее значение*/
            foreach (Film film in filmsDic.Values)
            {
                List<int> filmRatings = filmsDic[film.Id].Ratings;

                int totalScore = 0;
                foreach (int oneRating in filmRatings)
                {
                    switch( method )
                    {
                        case "average":
                            totalScore += oneRating;
                            break;

                        case "netvotes":
                            if (oneRating > 5)
                                totalScore++;
                            else
                                totalScore--;
                            break;
                        case "positive":
                             if (oneRating > 7)
                                totalScore++;
                            break;
                        default:
                            break;

                    }

                }

                switch( method )
                {
                    case "average":
                        film.Score = Convert.ToDouble(totalScore) / filmRatings.Count;
                        break;
                    case "netvotes":
                        film.Score = totalScore;
                        break;
                    case "positive":
                        film.Score = (Convert.ToDouble( totalScore ) / filmRatings.Count) * 100;
                        break;
                    default:
                        break;

                }
            }

            List<Film>list = filmsDic.Values.OrderByDescending(film => film.Score).ToList<Film>();
            using (StreamWriter writer = new StreamWriter(outputFilename))
            {
                writer.WriteLine("film_id|score");
                for (int i = 0; i <= 9; i++)
                {
                    Console.WriteLine("filmId: " + list[i].Id + " score " + Math.Round(list[i].Score, 2));
                    writer.WriteLine(list[i].Id + "|" + Math.Round(list[i].Score, 2).ToString());
                }
            }
        }    
    }
}
