﻿
namespace Demo.WindowsForms
{
#if SQLite
   using System.Collections.Generic;
   using System.Data.Common;
   using System.Data.SQLite;
   using System.IO;
   using System.Text;
   using System;
   using System.Diagnostics;

   /// <summary>
   /// ultra fast cache system for tiles
   /// </summary>
   internal class SQLiteIpCache
   {
      string _cache;
      string _ipCache;
      string _db;

      public string IpCache
      {
         get
         {
            return _ipCache;
         }
      }

      /// <summary>
      /// local cache location
      /// </summary>
      public string CacheLocation
      {
         get
         {
            return _cache;
         }
         set
         {
            _cache = value;
            _ipCache = Path.Combine(_cache, "IpGeoCacheDB") + Path.DirectorySeparatorChar;

            // make empty db
            {
               _db = _ipCache + "Data.ipdb";

               if(!File.Exists(_db))
               {
                  CreateEmptyDB(_db);
               }
            }
         }
      }

      public static bool CreateEmptyDB(string file)
      {
         bool ret = true;

         try
         {
            string dir = Path.GetDirectoryName(file);
            if(!Directory.Exists(dir))
            {
               Directory.CreateDirectory(dir);
            }

            using(SQLiteConnection cn = new SQLiteConnection())
            {
#if !MONO
               cn.ConnectionString = string.Format("Data Source=\"{0}\";FailIfMissing=False;", file);
#else
               cn.ConnectionString = string.Format("Version=3,URI=file://{0},FailIfMissing=False", file);
#endif
               cn.Open();
               {
                  using(DbTransaction tr = cn.BeginTransaction())
                  {
                     try
                     {
                        using(DbCommand cmd = cn.CreateCommand())
                        {
                           cmd.Transaction = tr;
                           cmd.CommandText = Properties.Resources.IpCacheCreateDb;
                           cmd.ExecuteNonQuery();
                        }
                        tr.Commit();
                     }
                     catch(Exception exx)
                     {
                        Console.WriteLine("CreateEmptyDB: " + exx.ToString());
                        Console.WriteLine("CreateEmptyDB: " + exx.ToString());

                        tr.Rollback();
                        ret = false;
                     }
                  }
                  cn.Close();
               }
            }
         }
         catch(Exception ex)
         {
#if MONO
            Console.WriteLine("CreateEmptyDB: " + ex.ToString());
#endif
            Console.WriteLine("CreateEmptyDB: " + ex.ToString());
            ret = false;
         }
         return ret;
      }

      public bool PutDataToCache(string ip, IpInfo data)
      {
         bool ret = true;
         try
         {
            using(SQLiteConnection cn = new SQLiteConnection())
            {
#if !MONO
               cn.ConnectionString = string.Format("Data Source=\"{0}\";", _db);
#else
               cn.ConnectionString = string.Format("Version=3,URI=file://{0},FailIfMissing=True,Default Timeout=33", _db);
#endif

               cn.Open();
               {
                  {
                     using(DbTransaction tr = cn.BeginTransaction())
                     {
                        try
                        {
                           using(DbCommand cmd = cn.CreateCommand())
                           {
                              cmd.Transaction = tr;

                              cmd.CommandText = "INSERT INTO Cache(Ip, CountryName, RegionName, City, Latitude, Longitude, Time) VALUES(@p1, @p2, @p3, @p4, @p5, @p6, @p7)";

                              cmd.Parameters.Add(new SQLiteParameter("@p1", ip));
                              cmd.Parameters.Add(new SQLiteParameter("@p2", data.CountryName));
                              cmd.Parameters.Add(new SQLiteParameter("@p3", data.RegionName));
                              cmd.Parameters.Add(new SQLiteParameter("@p4", data.City));
                              cmd.Parameters.Add(new SQLiteParameter("@p5", data.Latitude));
                              cmd.Parameters.Add(new SQLiteParameter("@p6", data.Longitude));
                              cmd.Parameters.Add(new SQLiteParameter("@p7", data.CacheTime));

                              cmd.ExecuteNonQuery();
                           }
                           tr.Commit();
                        }
                        catch(Exception ex)
                        {
                           Console.WriteLine("PutDataToCache: " + ex.ToString());

                           Console.WriteLine("PutDataToCache: " + ex.ToString());

                           tr.Rollback();
                           ret = false;
                        }
                     }
                  }
               }
               cn.Close();
            }
         }
         catch(Exception ex)
         {
#if MONO
            Console.WriteLine("PutDataToCache: " + ex.ToString());
#endif
            Console.WriteLine("PutDataToCache: " + ex.ToString());
            ret = false;
         }
         return ret;
      }

      public IpInfo GetDataFromCache(string ip)
      {
         IpInfo ret = null;
         try
         {
            using(SQLiteConnection cn = new SQLiteConnection())
            {
#if !MONO
               cn.ConnectionString = string.Format("Data Source=\"{0}\";", _db);
#else
               cn.ConnectionString = string.Format("Version=3,URI=file://{0},Default Timeout=33", _db);
#endif
               cn.Open();
               {
                  using(DbCommand com = cn.CreateCommand())
                  {
                     com.CommandText = "SELECT * FROM Cache WHERE Ip = '" + ip + "'";

                     using(DbDataReader rd = com.ExecuteReader())
                     {
                        if(rd.Read())
                        {
                           IpInfo val = new IpInfo();
                           {
                              val.Ip = ip;
                              val.CountryName = rd["CountryName"] as string;
                              val.RegionName = rd["RegionName"] as string;
                              val.City = rd["City"] as string;
                              val.Latitude = (double)rd["Latitude"];
                              val.Longitude = (double)rd["Longitude"];
                              val.CacheTime = (DateTime)rd["Time"];
                           }
                           ret = val;
                        }
                        rd.Close();
                     }
                  }
               }
               cn.Close();
            }
         }
         catch(Exception ex)
         {
#if MONO
            Console.WriteLine("GetDataFromCache: " + ex.ToString());
#endif
            Console.WriteLine("GetDataFromCache: " + ex.ToString());
            ret = null;
         }

         return ret;
      }
   }
#endif
}
