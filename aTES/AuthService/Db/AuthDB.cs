using LinqToDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AuthService.Db
{
    public class AuthDB : LinqToDB.Data.DataConnection
    {
        public AuthDB() : base("AuthDB")
        {
        }

        public ITable<Role> Roles => this.GetTable<Role>();
        public ITable<Parrot> Parrots  => this.GetTable<Parrot>();
    }
}