using System;
using System.Configuration;
using Microsoft.Data.SqlClient;

namespace Hastane_Otomasyonu.Database
{
    /// <summary>
    /// Veritabanı bağlantı ve işlemleri için merkezi sınıf
    /// </summary>
    public class SqlBaglantisi
    {
        private static SqlBaglantisi _instance;
        private SqlConnection _connection;
        private string _connectionString;

        /// <summary>
        /// Singleton pattern ile tek instance
        /// </summary>
        public static SqlBaglantisi Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new SqlBaglantisi();
                return _instance;
            }
        }

        /// <summary>
        /// Constructor - Connection string'i App.config'den alır
        /// </summary>
        private SqlBaglantisi()
        {
            try
            {
                _connectionString = ConfigurationManager.ConnectionStrings["HastaneOtomasyonuConnection"]?.ConnectionString;
                
                if (string.IsNullOrEmpty(_connectionString))
                {
                    // Eğer App.config'de bulunamazsa, varsayılan connection string kullan
                    _connectionString = "Server=(local);Database=Hastane;Integrated Security=True;TrustServerCertificate=True;";
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Connection string yüklenirken hata oluştu: {ex.Message}");
            }
        }

        /// <summary>
        /// Veritabanı bağlantısını açar ve döndürür
        /// </summary>
        public SqlConnection GetConnection()
        {
            try
            {
                if (_connection == null || _connection.State == System.Data.ConnectionState.Closed)
                {
                    _connection = new SqlConnection(_connectionString);
                    _connection.Open();
                }
                return _connection;
            }
            catch (SqlException ex)
            {
                throw new Exception($"Veritabanı bağlantısı açılamadı: {ex.Message}\n\n" +
                    $"SQL Server'ın çalıştığından ve '{_connectionString.Split(';')[1]}' veritabanının mevcut olduğundan emin olun.");
            }
        }

        /// <summary>
        /// Bağlantıyı kapatır
        /// </summary>
        public void CloseConnection()
        {
            if (_connection != null && _connection.State == System.Data.ConnectionState.Open)
            {
                _connection.Close();
                _connection.Dispose();
                _connection = null;
            }
        }

        /// <summary>
        /// Veritabanı bağlantısını test eder
        /// </summary>
        public bool TestConnection()
        {
            try
            {
                using (var conn = GetConnection())
                {
                    return conn.State == System.Data.ConnectionState.Open;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Dispose pattern
        /// </summary>
        public void Dispose()
        {
            CloseConnection();
        }
    }
}

