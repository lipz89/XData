﻿using NUnit.Framework;

using Winning.SPD.SCM.Domain;

using XData;

namespace Test
{
    public class TestJoin
    {
        [Test]
        public void Test()
        {
            var db = new XContext(Program.SqlConnectionString, Program.SqlProvider);
            var join = db.LeftJoin<Supplier, Cert>((x, y) => x.ID == y.SupplierID)
                .LeftJoin<CertPicture>((x, y) => x.Right.ID == y.CertID);
        }
    }
}