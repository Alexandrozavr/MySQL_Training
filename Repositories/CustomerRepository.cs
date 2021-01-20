using Dapper;
using HSEApiTraining.Models.Customer;
using HSEApiTraining.Providers;
using System;
using System.Collections.Generic;

namespace HSEApiTraining
{
    public interface ICustomerRepository
    {
        (IEnumerable<Customer> Customers, string Error) GetCustomers(int count);
        (IEnumerable<Customer> Customers, string Error) GetCustomersByName(string searchTerm);
        (IEnumerable<Customer> Customers, string Error) GetCustomersBySurname(string searchTerm);
        (IEnumerable<Customer> Customers, string Error) GetBannedCustomers();
        (IEnumerable<Customer> Customers, string Error) GetNotBannedCustomers();
        string AddCustomer(AddCustomerRequest request);
        string AddCustomers(int count);
        string DeleteCustomer(int id);
        string DeleteAllCustomers();
        string UpdateCustomer(int id, UpdateCustomerRequest request);
    }

    public class CustomerRepository : ICustomerRepository
    {
        private static Random rnd = new Random();

        private readonly ISQLiteConnectionProvider _connectionProvider;
        public CustomerRepository(ISQLiteConnectionProvider sqliteConnectionProvider)
        {
            _connectionProvider = sqliteConnectionProvider;
        }

        public (IEnumerable<Customer> Customers, string Error) GetCustomers(int count)
        {
            try
            {
                using (var connection = _connectionProvider.GetDbConnection())
                {
                    connection.Open();
                    if (count == -1)
                    {
                        return (
                            connection.Query<Customer>(@"
                        SELECT 
                        id as Id,
                        name as Name, 
                        surname as Surname, 
                        phone_number as PhoneNumber 
                        FROM Customer"),
                        null);
                    }
                    else
                    {
                        return (
                            connection.Query<Customer>(@"
                        SELECT 
                        id as Id,
                        name as Name, 
                        surname as Surname, 
                        phone_number as PhoneNumber 
                        FROM Customer 
                        LIMIT @count",
                            new { count = count }), null);
                    }
                }
            }
            catch (Exception e)
            {
                return (null, e.Message);
            }
        }

        public (IEnumerable<Customer> Customers, string Error) GetCustomersByName(string searchTerm)
        {
            try
            {
                using (var connection = _connectionProvider.GetDbConnection())
                {
                    connection.Open();
                    return (
                        connection.Query<Customer>(@"
                        SELECT 
                        id as Id,
                        name as Name, 
                        surname as Surname, 
                        phone_number as PhoneNumber 
                        FROM Customer
                        WHERE name LIKE @searchTerm",
                        new { searchTerm = '%' + searchTerm + '%' }),
                    null);
                }
            }
            catch (Exception e)
            {
                return (null, e.Message);
            }
        }

        public (IEnumerable<Customer> Customers, string Error) GetCustomersBySurname(string searchTerm)
        {
            try
            {
                using (var connection = _connectionProvider.GetDbConnection())
                {
                    connection.Open();
                    return (
                        connection.Query<Customer>(@"
                        SELECT 
                        id as Id,
                        name as Name, 
                        surname as Surname, 
                        phone_number as PhoneNumber 
                        FROM Customer
                        WHERE surname LIKE @searchTerm",
                        new { searchTerm = '%' + searchTerm + '%' }),
                    null);
                }
            }
            catch (Exception e)
            {
                return (null, e.Message);
            }
        }

        public (IEnumerable<Customer> Customers, string Error) GetBannedCustomers()
        {
            try
            {
                using (var connection = _connectionProvider.GetDbConnection())
                {
                    connection.Open();
                    return (
                        connection.Query<Customer>(@"
                        SELECT DISTINCT
                        Customer.id as Id,
                        Customer.name as Name, 
                        Customer.surname as Surname, 
                        Customer.phone_number as PhoneNumber 
                        FROM Customer join banned_phone
                        ON Customer.phone_number = banned_phone.phone"),
                    null);
                }
            }
            catch (Exception e)
            {
                return (null, e.Message);
            }
        }

        public (IEnumerable<Customer> Customers, string Error) GetNotBannedCustomers()
        {
            try
            {
                using (var connection = _connectionProvider.GetDbConnection())
                {
                    connection.Open();
                    return (
                        connection.Query<Customer>(@"
                        SELECT 
                        Customer.id as Id,
                        Customer.name as Name, 
                        Customer.surname as Surname, 
                        Customer.phone_number as PhoneNumber 
                        FROM Customer join(
                        SELECT 
                        Customer.phone_number as PhoneNumber 
                        FROM Customer 
                        EXCEPT
                        SELECT phone 
                        FROM banned_phone)
                        WHERE Customer.phone_number = PhoneNumber"),
                    null);
                }
            }
            catch (Exception e)
            {
                return (null, e.Message);
            }
        }

        public string AddCustomer(AddCustomerRequest request)
        {
            try
            {
                if(request.PhoneNumber.Length < 10 || request.PhoneNumber.Length > 14 || 
                    request.PhoneNumber[0] != '+' || !(request.PhoneNumber.Contains("+1") ||
                       request.PhoneNumber.Contains("+0") || request.PhoneNumber.Contains("+380") || request.PhoneNumber.Contains("+7")))
                {
                    throw new Exception("Ти шо за бяку запихнул?");
                }
                using (var connection = _connectionProvider.GetDbConnection())
                {
                    connection.Open();
                    connection.Execute(
                        @"INSERT INTO Customer 
                        ( name, surname, phone_number ) VALUES 
                        ( @Name, @Surname, @PhoneNumber );",
                        new { Name = request.Name, Surname = request.Surname, PhoneNumber = request.PhoneNumber });
                }
                return null;
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        public string AddCustomers(int count)
        {
            try
            {
                for (int i = 0; i < count; i++)
                {
                    using (var connection = _connectionProvider.GetDbConnection())
                    {
                        connection.Open();
                        connection.Execute(
                            @"INSERT INTO Customer 
                        ( name, surname, phone_number ) VALUES 
                        ( @Name, @Surname, @PhoneNumber );",
                            new
                            {
                                Name = names[rnd.Next(10)],
                                Surname = surnames[rnd.Next(10)],
                                PhoneNumber = NumberGen()
                            });
                    }
                }
                return null;
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        public string DeleteCustomer(int id)
        {
            try
            {
                using (var connection = _connectionProvider.GetDbConnection())
                {
                    connection.Open();
                    if (0 == connection.Execute(
                        @"DELETE FROM Customer WHERE id = @Id;",
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

        public string DeleteAllCustomers()
        {
            try
            {
                using (var connection = _connectionProvider.GetDbConnection())
                {
                    connection.Open();
                    connection.Execute(@"DELETE FROM Customer");
                }
                return null;
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        public string UpdateCustomer(int id, UpdateCustomerRequest request)
        {
            try
            {
                using (var connection = _connectionProvider.GetDbConnection())
                {
                    connection.Open();
                    if (0 == connection.Execute(
                        @"UPDATE Customer 
                        SET name = @Name, surname = @Surname, phone_number = @PhoneNumber
                        WHERE id = @Id;",
                        new { Name = request.Name, Surname = request.Surname, PhoneNumber = request.PhoneNumber, Id = id }))
                    {
                        throw new Exception("Нечего редактировать!");
                    }
                }
                return null;
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }
        static string NumberGen()
        {
            string user_number = "";
            string numbers = "0123456789";
            switch (rnd.Next(4))
            {
                case 0:
                    {
                        user_number = "+7";
                        break;
                    }
                case 1:
                    {
                        user_number = "+0";
                        break;
                    }
                case 2:
                    {
                        user_number = "+380";
                        break;
                    }
                case 3:
                    {
                        user_number = "+1";
                        break;
                    }
            }
            for (int i = 0; i < 8; i++)
            {
                user_number += numbers[rnd.Next(numbers.Length)];
            }
            return user_number;
        }

        readonly string[] names =
        {
            "Arjac", "Logan", "Roboute", "Cawl", "Canis", "Harald", "Krom", "Njal", "Ragnar", "Ulrik"
        };
        readonly string[] surnames =
        {
            "Rockfist", "Grimnar", "Guilliman", "Belisarius", "Wolfborn", "Deathwolf", "Dragongaze", "Stormcaller", "Blackmane", "Slayer"
        };
    }
}
