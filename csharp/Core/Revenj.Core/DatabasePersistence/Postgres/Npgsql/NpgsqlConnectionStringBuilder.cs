// created on 29/11/2007
// Npgsql.NpgsqlConnectionStringBuilder.cs
//
// Author:
//	Glen Parker (glenebob@nwlink.com)
//	Ben Sagal (bensagal@gmail.com)
//	Tao Wang (dancefire@gmail.com)
//
//	Copyright (C) 2007 The Npgsql Development Team
//	npgsql-general@gborg.postgresql.org
//	http://gborg.postgresql.org/project/npgsql/projdisplay.php
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
// 
// IN NO EVENT SHALL THE NPGSQL DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE NPGSQL DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
// 
// THE NPGSQL DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE NPGSQL DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection;

namespace Revenj.DatabasePersistence.Postgres.Npgsql
{
	public sealed class NpgsqlConnectionStringBuilder : DbConnectionStringBuilder
	{
		private static readonly Dictionary<Keywords, object> defaults = new Dictionary<Keywords, object>();

		private string originalConnectionString;

		private const int POOL_SIZE_LIMIT = 1024;
		private const int TIMEOUT_LIMIT = 1024;
		internal const int DefaultCommandTimeout = 20;

		static NpgsqlConnectionStringBuilder()
		{
			defaults.Add(Keywords.Host, string.Empty);
			defaults.Add(Keywords.Port, 5432);
			defaults.Add(Keywords.Database, string.Empty);
			defaults.Add(Keywords.UserName, string.Empty);
			defaults.Add(Keywords.Password, string.Empty);
			defaults.Add(Keywords.SSL, false);
			defaults.Add(Keywords.SslMode, SslMode.Disable);
			defaults.Add(Keywords.Timeout, 15);
			defaults.Add(Keywords.SearchPath, string.Empty);
			defaults.Add(Keywords.Pooling, true);
			defaults.Add(Keywords.ConnectionLifeTime, 15);
			defaults.Add(Keywords.MinPoolSize, 1);
			defaults.Add(Keywords.MaxPoolSize, 20);
			defaults.Add(Keywords.SyncNotification, false);
			defaults.Add(Keywords.CommandTimeout, DefaultCommandTimeout);
			defaults.Add(Keywords.Enlist, false);
			defaults.Add(Keywords.PreloadReader, false);
			defaults.Add(Keywords.UseExtendedTypes, false);
			defaults.Add(Keywords.IntegratedSecurity, false);
			defaults.Add(Keywords.Compatible, THIS_VERSION);
			defaults.Add(Keywords.ApplicationName, string.Empty);
		}

		public NpgsqlConnectionStringBuilder()
		{
			this.Clear();
		}

		public NpgsqlConnectionStringBuilder(string connectionString)
		{
			this.Clear();
			this.originalConnectionString = connectionString;
			base.ConnectionString = connectionString;
		}

		/// <summary>
		/// Return an exact copy of this NpgsqlConnectionString.
		/// </summary>
		public NpgsqlConnectionStringBuilder Clone()
		{
			NpgsqlConnectionStringBuilder builder = new NpgsqlConnectionStringBuilder();
			foreach (string key in this.Keys)
			{
				builder[key] = this[key];
			}
			return builder;
		}

		#region Parsing Functions

		private static SslMode ToSslMode(object value)
		{
			if (value is SslMode)
			{
				return (SslMode)value;
			}
			else
			{
				return (SslMode)Enum.Parse(typeof(SslMode), value.ToString(), true);
			}
		}

		private static int ToInt32(object value, int min, int max, string key)
		{
			int v = Convert.ToInt32(value);
			if (v < min)
			{
				throw new ArgumentOutOfRangeException(String.Format("numeric value {0} in ConnectionString is below minimum value {1}", key, min), key);
			}
			else if (v > max)
			{
				throw new ArgumentOutOfRangeException(String.Format("numeric value {0} in ConnectionString exceeds maximum value {1}", key, max), key);
			}
			return v;
		}

		private static Boolean ToBoolean(object value)
		{
			string text = value as string;
			if (text != null)
			{
				switch (text.ToLowerInvariant())
				{
					case "t":
					case "true":
					case "y":
					case "yes":
						return true;

					case "f":
					case "false":
					case "n":
					case "no":
						return false;

					default:
						throw new InvalidCastException(value.ToString());
				}
			}
			else
			{
				return Convert.ToBoolean(value);
			}
		}

		private Boolean ToIntegratedSecurity(object value)
		{
			string text = value as string;
			if (text != null)
			{
				switch (text.ToLowerInvariant())
				{
					case "t":
					case "true":
					case "y":
					case "yes":
					case "sspi":
						return true;

					case "f":
					case "false":
					case "n":
					case "no":
						return false;

					default:
						throw new InvalidCastException(value.ToString());
				}
			}
			else
			{
				return Convert.ToBoolean(value);
			}
		}

		#endregion

		#region Properties

		private string _host;

		public string Host
		{
			get { return _host; }
			set { SetValue(GetKeyName(Keywords.Host), value); }
		}

		private int _port;

		public int Port
		{
			get { return _port; }
			set { SetValue(GetKeyName(Keywords.Port), value); }
		}

		private string _database;

		public string Database
		{
			get { return _database; }
			set { SetValue(GetKeyName(Keywords.Database), value); }
		}

		private string _username;

		public string UserName
		{
			get
			{
				if ((_integrated_security) && (String.IsNullOrEmpty(_username)))
				{
					
					System.Security.Principal.WindowsIdentity identity = System.Security.Principal.WindowsIdentity.GetCurrent();
					_username = identity.Name.Split('\\')[1];
				}
				return _username;
			}
			set { SetValue(GetKeyName(Keywords.UserName), value); }
		}

		private byte[] _password;

		public byte[] PasswordAsByteArray
		{
			get { return _password; }
			set { _password = value; }
		}

		public string Password
		{
			set { SetValue(GetKeyName(Keywords.Password), value); }
		}

		private bool _ssl;

		public bool SSL
		{
			get { return _ssl; }
			set { SetValue(GetKeyName(Keywords.SSL), value); }
		}

		private SslMode _sslmode;

		public SslMode SslMode
		{
			get { return _sslmode; }
			set { SetValue(GetKeyName(Keywords.SslMode), value); }
		}

		[Obsolete("UTF-8 is always used regardless of this setting.")]
		public string Encoding
		{
			get { return "UNICODE"; }
			//set { }
		}

		private int _timeout;

		public int Timeout
		{
			get { return _timeout; }
			set { SetValue(GetKeyName(Keywords.Timeout), value); }
		}

		private string _searchpath;

		public string SearchPath
		{
			get { return _searchpath; }
			set { SetValue(GetKeyName(Keywords.SearchPath), value); }
		}

		private bool _pooling;

		public bool Pooling
		{
			get { return _pooling; }
			set { SetValue(GetKeyName(Keywords.Pooling), value); }
		}

		private int _connection_life_time;

		public int ConnectionLifeTime
		{
			get { return _connection_life_time; }
			set { SetValue(GetKeyName(Keywords.ConnectionLifeTime), value); }
		}

		private int _min_pool_size;

		public int MinPoolSize
		{
			get { return _min_pool_size; }
			set { SetValue(GetKeyName(Keywords.MinPoolSize), value); }
		}

		private int _max_pool_size;

		public int MaxPoolSize
		{
			get { return _max_pool_size; }
			set { SetValue(GetKeyName(Keywords.MaxPoolSize), value); }
		}

		private bool _sync_notification;

		public bool SyncNotification
		{
			get { return _sync_notification; }
			set { SetValue(GetKeyName(Keywords.SyncNotification), value); }
		}

		private int _command_timeout;

		public int CommandTimeout
		{
			get { return _command_timeout; }
			set { SetValue(GetKeyName(Keywords.CommandTimeout), value); }
		}

		private bool _enlist;

		public bool Enlist
		{
			get { return _enlist; }
			set { SetValue(GetKeyName(Keywords.Enlist), value); }
		}

		private bool _preloadReader;

		public bool PreloadReader
		{
			get { return _preloadReader; }
			set { SetValue(GetKeyName(Keywords.PreloadReader), value); }
		}

		private bool _useExtendedTypes;

		public bool UseExtendedTypes
		{
			get { return _useExtendedTypes; }
			set { SetValue(GetKeyName(Keywords.UseExtendedTypes), value); }
		}

		private bool _integrated_security;

		public bool IntegratedSecurity
		{
			get { return _integrated_security; }
			set { SetValue(GetKeyName(Keywords.IntegratedSecurity), value); }
		}

		private Version _compatible;
		private static readonly Version THIS_VERSION = MethodBase.GetCurrentMethod().DeclaringType.Assembly.GetName().Version;
		/// <summary>
		/// Compatibilty version. When possible, behaviour caused by breaking changes will be preserved
		/// if this version is less than that where the breaking change was introduced.
		/// </summary>
		public Version Compatible
		{
			get { return _compatible; }
			set { SetValue(GetKeyName(Keywords.Compatible), value); }
		}

		private string _application_name;

		public string ApplicationName
		{
			get { return _application_name; }
			set { SetValue(GetKeyName(Keywords.ApplicationName), value); }
		}

		#endregion

		private static Keywords GetKey(string key)
		{
			switch (key.ToUpperInvariant())
			{
				case "HOST":
				case "SERVER":
					return Keywords.Host;
				case "PORT":
					return Keywords.Port;
				case "DATABASE":
				case "DB":
					return Keywords.Database;
				case "USERNAME":
				case "USER NAME":
				case "USER":
				case "USERID":
				case "USER ID":
				case "UID":
					return Keywords.UserName;
				case "PASSWORD":
				case "PSW":
				case "PWD":
					return Keywords.Password;
				case "SSL":
					return Keywords.SSL;
				case "SSLMODE":
					return Keywords.SslMode;
				case "ENCODING":
#pragma warning disable 618
					return Keywords.Encoding;
#pragma warning restore 618
				case "TIMEOUT":
					return Keywords.Timeout;
				case "SEARCHPATH":
					return Keywords.SearchPath;
				case "POOLING":
					return Keywords.Pooling;
				case "CONNECTIONLIFETIME":
					return Keywords.ConnectionLifeTime;
				case "MINPOOLSIZE":
					return Keywords.MinPoolSize;
				case "MAXPOOLSIZE":
					return Keywords.MaxPoolSize;
				case "SYNCNOTIFICATION":
					return Keywords.SyncNotification;
				case "COMMANDTIMEOUT":
					return Keywords.CommandTimeout;
				case "ENLIST":
					return Keywords.Enlist;
				case "PRELOADREADER":
				case "PRELOAD READER":
					return Keywords.PreloadReader;
				case "USEEXTENDEDTYPES":
				case "USE EXTENDED TYPES":
					return Keywords.UseExtendedTypes;
				case "INTEGRATED SECURITY":
					return Keywords.IntegratedSecurity;
				case "COMPATIBLE":
					return Keywords.Compatible;
				case "APPLICATIONNAME":
					return Keywords.ApplicationName;
				default:
					throw new ArgumentException("key=value argument incorrect in ConnectionString", key);
			}
		}

		internal static string GetKeyName(Keywords keyword)
		{
			switch (keyword)
			{
				case Keywords.Host:
					return "HOST";
				case Keywords.Port:
					return "PORT";
				case Keywords.Database:
					return "DATABASE";
				case Keywords.UserName:
					return "USER ID";
				case Keywords.Password:
					return "PASSWORD";
				case Keywords.SSL:
					return "SSL";
				case Keywords.SslMode:
					return "SSLMODE";
				case Keywords.Encoding:
					return "ENCODING";
				case Keywords.Timeout:
					return "TIMEOUT";
				case Keywords.SearchPath:
					return "SEARCHPATH";
				case Keywords.Pooling:
					return "POOLING";
				case Keywords.ConnectionLifeTime:
					return "CONNECTIONLIFETIME";
				case Keywords.MinPoolSize:
					return "MINPOOLSIZE";
				case Keywords.MaxPoolSize:
					return "MAXPOOLSIZE";
				case Keywords.SyncNotification:
					return "SYNCNOTIFICATION";
				case Keywords.CommandTimeout:
					return "COMMANDTIMEOUT";
				case Keywords.Enlist:
					return "ENLIST";
				case Keywords.PreloadReader:
					return "PRELOADREADER";
				case Keywords.UseExtendedTypes:
					return "USEEXTENDEDTYPES";
				case Keywords.IntegratedSecurity:
					return "INTEGRATED SECURITY";
				case Keywords.Compatible:
					return "COMPATIBLE";
				default:
					return keyword.ToString().ToUpperInvariant();
			}
		}

		internal static object GetDefaultValue(Keywords keyword)
		{
			return defaults[keyword];
		}

		/// <summary>
		/// Case insensative accessor for indivual connection string values.
		/// </summary>
		public override object this[string keyword]
		{
			get { return this[GetKey(keyword)]; }
			set { this[GetKey(keyword)] = value; }
		}

		public object this[Keywords keyword]
		{
			get { return base[GetKeyName(keyword)]; }
			set { SetValue(GetKeyName(keyword), value); }
		}

		public override bool Remove(string keyword)
		{
			Keywords key = GetKey(keyword);
			SetValue(key, defaults[key]);
			return base.Remove(keyword);
		}

		public bool ContainsKey(Keywords keyword)
		{
			return base.ContainsKey(GetKeyName(keyword));
		}

		/// <summary>
		/// This function will set value for known key, both private member and base[key].
		/// </summary>
		/// <param name="keyword"></param>
		/// <param name="value"></param>
		private void SetValue(string keyword, object value)
		{
			if (value == null)
			{
				Remove(keyword);
			}
			else
			{
				Keywords key = GetKey(keyword);
				SetValue(key, value);
				if (key == Keywords.Compatible)
				{
					base[GetKeyName(key)] = ((Version)this.Compatible).ToString();
				}
				else
				{
					base[GetKeyName(key)] = value;
				}
			}
		}

		/// <summary>
		/// The function will modify private member only, not base[key].
		/// </summary>
		/// <param name="keyword"></param>
		/// <param name="value"></param>
		private void SetValue(Keywords keyword, object value)
		{
			string key_name = GetKeyName(keyword);

			try
			{
				switch (keyword)
				{
					case Keywords.Host:
						this._host = Convert.ToString(value);
						break;
					case Keywords.Port:
						this._port = Convert.ToInt32(value);
						break;
					case Keywords.Database:
						this._database = Convert.ToString(value);
						break;
					case Keywords.UserName:
						this._username = Convert.ToString(value);
						break;
					case Keywords.Password:
						this._password = System.Text.Encoding.UTF8.GetBytes(Convert.ToString(value));
						break;
					case Keywords.SSL:
						this._ssl = ToBoolean(value);
						break;
					case Keywords.SslMode:
						this._sslmode = ToSslMode(value);
						break;
#pragma warning disable 618
					case Keywords.Encoding:
						break;
#pragma warning restore 618
					case Keywords.Timeout:
						this._timeout = ToInt32(value, 0, TIMEOUT_LIMIT, key_name);
						break;
					case Keywords.SearchPath:
						this._searchpath = Convert.ToString(value);
						break;
					case Keywords.Pooling:
						this._pooling = ToBoolean(value);
						break;
					case Keywords.ConnectionLifeTime:
						this._connection_life_time = Convert.ToInt32(value);
						break;
					case Keywords.MinPoolSize:
						this._min_pool_size = (MaxPoolSize > 0) ? ToInt32(value, 0, MaxPoolSize, key_name) : Convert.ToInt32(value);
						break;
					case Keywords.MaxPoolSize:
						this._max_pool_size = ToInt32(value, 0, POOL_SIZE_LIMIT, key_name);
						break;
					case Keywords.SyncNotification:
						this._sync_notification = ToBoolean(value);
						break;
					case Keywords.CommandTimeout:
						this._command_timeout = Convert.ToInt32(value);
						break;
					case Keywords.Enlist:
						this._enlist = ToBoolean(value);
						break;
					case Keywords.PreloadReader:
						this._preloadReader = ToBoolean(value);
						break;
					case Keywords.UseExtendedTypes:
						this._useExtendedTypes = ToBoolean(value);
						break;
					case Keywords.IntegratedSecurity:
						this._integrated_security = ToIntegratedSecurity(value);
						break;
					case Keywords.Compatible:
						Version ver = new Version(value.ToString());
						if (ver > THIS_VERSION)
							throw new ArgumentException("Attempt to set compatibility with version " + value + " when using version " + THIS_VERSION);
						_compatible = ver;
						break;
					case Keywords.ApplicationName:
						this._application_name = Convert.ToString(value);
						break;
				}
			}
			catch (InvalidCastException exception)
			{
				string exception_template = string.Empty;
				switch (keyword)
				{
					case Keywords.Port:
					case Keywords.Timeout:
					case Keywords.ConnectionLifeTime:
					case Keywords.MinPoolSize:
					case Keywords.MaxPoolSize:
					case Keywords.CommandTimeout:
						exception_template = "expecting {0}=[Numeric] value in ConnectionString";
						break;
					case Keywords.SSL:
					case Keywords.Pooling:
					case Keywords.SyncNotification:
						exception_template = "expecting {0}=[True/False] value in ConnectionString";
						break;
				}
				if (!string.IsNullOrEmpty(exception_template))
				{
					throw new ArgumentException(string.Format(exception_template, key_name), key_name, exception);
				}
				throw;
			}
		}

		/// <summary>
		/// Clear the member and assign them to the default value.
		/// </summary>
		public override void Clear()
		{
			base.Clear();
			foreach (Keywords keyword in defaults.Keys)
			{
				SetValue(GetKeyName(keyword), defaults[keyword]);
			}
		}
	}

	public enum Keywords
	{
		Host,
		Port,
		Database,
		UserName,
		Password,
		SSL,
		SslMode,
		[Obsolete("UTF-8 is always used regardless of this setting.")]
		Encoding,
		Timeout,
		SearchPath,
		//	These are for the connection pool
		Pooling,
		ConnectionLifeTime,
		MinPoolSize,
		MaxPoolSize,
		SyncNotification,
		// These are for the command
		CommandTimeout,
		// These are for the resource manager
		Enlist,
		PreloadReader,
		UseExtendedTypes,
		IntegratedSecurity,
		Compatible,
		ApplicationName
	}

	public enum SslMode
	{
		Disable = 1 << 0,
		Allow = 1 << 1,
		Prefer = 1 << 2,
		Require = 1 << 3
	}
}
