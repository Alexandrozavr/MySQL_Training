using Dapper;
using HSEApiTraining.Models.Ban;
using HSEApiTraining.Providers;
using System;
using System.Collections.Generic;

namespace HSEApiTraining
{
    public interface IBanRepository
    {
        string BanPhone(string phone);
        (IEnumerable<BannedPhone> Banned_phones, string Error) GetAllBanned();
        string DeletePhone(int id);
        string DeleteAllPhones();
    }

    public class BanRepository : IBanRepository
    {
        private readonly ISQLiteConnectionProvider _connectionProvider;

        public BanRepository(ISQLiteConnectionProvider sqliteConnectionProvider)
        {
            _connectionProvider = sqliteConnectionProvider;
        }

        public string BanPhone(string phone)
        {
            try
            {
                using (var connection = _connectionProvider.GetDbConnection())
            {
                connection.Open();

                if (0 == connection.Execute(
                    @"UPDATE banned_phone 
                        SET phone = @PhoneNumber  
                        WHERE phone = @PhoneNumber;", new { PhoneNumber = phone }))
                {
                    connection.Execute(@"INSERT INTO banned_phone 
                                            (phone) VALUES 
                                            (@PhoneNumber);",
                        new
                        {
                            PhoneNumber = phone
                        });
                    return null;
                }
                else
                {
                    return $"This number is already exist";
                }

                }
            }
            catch (Exception e)
            {
                return e.Message;
            }

        }

        public string DeleteAllPhones()
        {
            try
            {
                using (var connection = _connectionProvider.GetDbConnection())
                {
                    connection.Open();
                    connection.Execute(@"DELETE FROM banned_phone");
                }
                return null;
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        public string DeletePhone(int id)
        {
            try
            {
                using (var connection = _connectionProvider.GetDbConnection())
                {
                    connection.Open();
                    if (0 == connection.Execute(
                        @"DELETE FROM banned_phone WHERE id = @Id;",
                        new { Id = id }))
                    {
                        throw new Exception("Нечего удалять!");
                    }

                }
                return null;
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        public (IEnumerable<BannedPhone> Banned_phones, string Error) GetAllBanned()
        {
            try
            {
                using (var connection = _connectionProvider.GetDbConnection())
                {
                    connection.Open();
                    return (
                        connection.Query<BannedPhone>(@"
                        SELECT 
                        id as Id,
                        phone as Number 
                        FROM banned_phone"),
                    null);

                }
            }
            catch (Exception e)
            {
                return (null, e.Message);
            }
        }
    }
}
