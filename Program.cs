using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;

namespace RSDZ1
{
    class Film
    {
        public string Id;
        public double Score;
        public List<int> Ratings = new List<int>();
        public Dictionary<string, string> Users = new Dictionary<string, string>();
        public string Name;

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

            string filmIdForAssociation="";
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
          
            Dictionary<string, string> allUsers = new Dictionary<string, string>();

            #region считывание данных из файла
            int i = 0;

            Console.WriteLine(DateTime.Now.ToString());
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
                            continue;
                        }

                        int rating = Convert.ToInt32(curRow[2]);

                        if (!filmsDic.ContainsKey(filmId))
                        {

                            Film film = new Film( filmId );
                            filmsDic.Add(filmId, film);
                        }
 
                        if (!filmsDic[filmId].Users.ContainsKey(userId))
                        {
                            filmsDic[filmId].Users.Add(userId, userId);

                           filmsDic[filmId].Ratings.Add(rating);
                        }

                       

                        if (!allUsers.ContainsKey(userId))
                            allUsers.Add(userId, userId);
                        i++;
                    }
                    else
                        first = false;

                }
            }

            /*считывание названий фильмов*/
            using (StreamReader stream = new StreamReader(titlesFilename))
            { 
                 bool first = true;
                 while (!stream.EndOfStream)
                 {
                     string[] curRow = stream.ReadLine().Split('|');
                     if (!first) /*строка заголовка не интересует*/
                     {
                         string filmId = curRow[0];
                         string filmName = curRow[1];
                         filmsDic[filmId].Name = filmName;
                     }
                     else
                         first = false;
                 }
            }

            Console.WriteLine( DateTime.Now.ToString() + " Данные считаны");
            #endregion

            Console.WriteLine( "nUsers " + allUsers.Count.ToString());
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
                case "association":
                    FindAssociation(filmsDic, outputFilename, filmIdForAssociation, allUsers);
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
                    Console.WriteLine("filmId: " + list[i].Id + " score " + Math.Round(list[i].Score, 2) + " filmName " + list[i].Name);
                    writer.WriteLine(list[i].Id + "|" + Math.Round(list[i].Score, 2).ToString() );
                }
            }
        }

        public static void FindAssociation(Dictionary<string, Film> filmsDic, string outputFilename, string filmId, Dictionary<string, string> allUsers)
        {  
            Console.WriteLine( "смотрели X " +  filmsDic[filmId].Users.Count.ToString() );
            List<string> notX = allUsers.Keys.Except<string>(filmsDic[filmId].Users.Keys).ToList<string>();
            Console.WriteLine( "не смотрели X:" + notX.Count.ToString());
             /* для каждого фильма находим значение ассоциации с заданным фильмом*/
            foreach (Film film in filmsDic.Values)
            {
                if (film.Id != filmId)
                {
                    List<string> XYIntersection = film.Users.Keys.Intersect<string>(filmsDic[filmId].Users.Keys).ToList<string>();

                    List<string> notXYIntersection = film.Users.Keys.Intersect<string>(notX).ToList<string>();

                    film.Score = (Convert.ToDouble(XYIntersection.Count) / filmsDic[filmId].Users.Count) / ( Convert.ToDouble( notXYIntersection.Count ) / notX.Count);
                
                }
            }

            List<Film> list = filmsDic.Values.OrderByDescending(film => film.Score).ToList<Film>();
            using (StreamWriter writer = new StreamWriter(outputFilename))
            {
                writer.WriteLine("film_id|score");
                for (int i = 0; i <= 9; i++)
                {
                    Console.WriteLine("filmId: " + list[i].Id + " score " + Math.Round(list[i].Score, 2) + " filmName " + list[i].Name );
                    writer.WriteLine(list[i].Id + "|" + Math.Round(list[i].Score, 2).ToString());
                }
            }
        
        }
    }
}
