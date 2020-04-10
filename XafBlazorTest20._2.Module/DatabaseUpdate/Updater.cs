using System;
using System.Linq;
using DevExpress.ExpressApp;
using DevExpress.Data.Filtering;
using DevExpress.Persistent.Base;
using DevExpress.ExpressApp.Updating;
using DevExpress.Xpo;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.BaseImpl;
using System.Globalization;
using Demo1.Module.BusinessObjects;
using Bogus;
using System.Collections.Generic;

namespace XafBlazorTest20._2.Module.DatabaseUpdate
{
    // For more typical usage scenarios, be sure to check out https://docs.devexpress.com/eXpressAppFramework/DevExpress.ExpressApp.Updating.ModuleUpdater
    public class Updater : ModuleUpdater
    {
        public Updater(IObjectSpace objectSpace, Version currentDBVersion) :
            base(objectSpace, currentDBVersion)
        {
        }
        public override void UpdateDatabaseAfterUpdateSchema()
        {
            base.UpdateDatabaseAfterUpdateSchema();
            //string name = "MyName";
            //DomainObject1 theObject = ObjectSpace.FindObject<DomainObject1>(CriteriaOperator.Parse("Name=?", name));
            //if(theObject == null) {
            //    theObject = ObjectSpace.CreateObject<DomainObject1>();
            //    theObject.Name = name;
            //}
            DodajKraje(ObjectSpace);

         //   WygenerujKlientow();

 
            ObjectSpace.CommitChanges(); //Uncomment this line to persist created object(s).
        }

        private void WygenerujKlientow()
        {
            var cusFaker = new Faker<Klient>("pl")
              .CustomInstantiator(f => ObjectSpace.CreateObject<Klient>())
              .RuleFor(o => o.Telefon, f => f.Person.Phone)
              .RuleFor(o => o.Skrot, f => f.Company.CompanyName())
              .RuleFor(o => o.Nazwa, f => f.Company.CompanyName())
              .RuleFor(o => o.Email, (f, u) => f.Internet.Email())
              .RuleFor(o => o.Miejscowosc, f => f.Address.City())
              .RuleFor(o => o.KodPocztowy, f => f.Address.ZipCode())
              .RuleFor(o => o.Ulica, f => f.Address.StreetName());
            var customers = cusFaker.Generate(1000);
            ObjectSpace.CommitChanges();

            var conFaker = new Faker<Kontakt>("pl")
            .CustomInstantiator(f => ObjectSpace.CreateObject<Kontakt>())
            .RuleFor(o => o.Imie, f => f.Person.FirstName)
            .RuleFor(o => o.Nazwisko, f => f.Person.LastName)
            .RuleFor(o => o.Klient, f => f.PickRandom(cusFaker))
            .RuleFor(o => o.Email, (f, u) => f.Internet.Email())
            .RuleFor(o => o.Telefon, f => f.Person.Phone);

            var contacts = conFaker.Generate(10000);

            var stawki = new List<StawkaVAT>();
            stawki.Add(NowaStawka("23%", 23M));
            stawki.Add(NowaStawka("0%", 0M));
            stawki.Add(NowaStawka("7%", 7M));
            stawki.Add(NowaStawka("ZW", 0M));



            var prodFaker = new Faker<Produkt>("pl")
             .CustomInstantiator(f => ObjectSpace.CreateObject<Produkt>())
             .RuleFor(o => o.Nazwa, f => f.Commerce.ProductName())
             .RuleFor(o => o.StawkaVAT, f => f.PickRandom(stawki))
             .RuleFor(o => o.Cena, f => f.Random.Decimal(0.01M, 1000M));

            var products = prodFaker.Generate(100);
            WygenerujFaktury(10000, customers, products);
        }

        public override void UpdateDatabaseBeforeUpdateSchema()
        {
            base.UpdateDatabaseBeforeUpdateSchema();
            //if(CurrentDBVersion < new Version("1.1.0.0") && CurrentDBVersion > new Version("0.0.0.0")) {
            //    RenameColumn("DomainObject1Table", "OldColumnName", "NewColumnName");
            //}
        }

        private StawkaVAT NowaStawka(string symbol, decimal wartosc)
        {

            var stawka = ObjectSpace.FindObject<StawkaVAT>(new BinaryOperator("Symbol", symbol));
            if (stawka == null)
            {
                stawka = ObjectSpace.CreateObject<StawkaVAT>();
                stawka.Symbol = symbol;
                stawka.Stawka = wartosc;

            }
            return stawka;
        }

        private void WygenerujFaktury(int liczbaFaktur, IList<Klient> customers, IList<Produkt> products)
        {
            if (customers is null)
            {
                customers = ObjectSpace.GetObjectsQuery<Klient>().ToList();
            }

            var orderFaker = new Faker<Faktura>("pl")
            .CustomInstantiator(f => ObjectSpace.CreateObject<Faktura>())
            .RuleFor(o => o.NumerFaktury, f => f.Random.Int().ToString())
            .RuleFor(o => o.DataFaktury, f => f.Date.Past(2))
            .RuleFor(o => o.DataSprzedazy, f => f.Date.Past(20))
            .RuleFor(o => o.Klient, f => f.PickRandom(customers));
            var orders = orderFaker.Generate(liczbaFaktur);
            if (products == null)
            {
                products = ObjectSpace.GetObjectsQuery<Produkt>().ToList();
            }

            var itemsFaker = new Faker<PozycjaFaktury>()
            .CustomInstantiator(f => ObjectSpace.CreateObject<PozycjaFaktury>())
            .RuleFor(o => o.Faktura, f => f.PickRandom(orders))
            .RuleFor(o => o.Produkt, f => f.PickRandom(products))

            .RuleFor(o => o.Ilosc, f => f.Random.Decimal(0.01M, 100M));

            var items = itemsFaker.Generate(liczbaFaktur * 10);
        }

        void DodajKraje(IObjectSpace os)
        {

            foreach (CultureInfo ci in CultureInfo.GetCultures(CultureTypes.FrameworkCultures))

            {

                RegionInfo ri = null;

                try

                {

                    ri = new RegionInfo(ci.Name);

                }

                catch

                {

                    // If a RegionInfo object could not be created we don't want to use the CultureInfo

                    //    for the country list.

                    continue;

                }

                // var kraj =    os.CreateObject<Kraj>();



                var a = ri.EnglishName;
                var a1 = ri.NativeName;
                var a2 = ri.ThreeLetterISORegionName;
                var a3 = ri.CurrencyEnglishName;
                var a4 = ri.CurrencyNativeName;
                var a5 = ri.CurrencySymbol;
                var a6 = ri.ISOCurrencySymbol;

                var waluta = ObjectSpace.FindObject<Waluta>(new BinaryOperator("Symbol", ri.ISOCurrencySymbol));
                if (waluta == null)
                {
                    waluta = ObjectSpace.CreateObject<Waluta>();
                    waluta.Symbol = ri.ISOCurrencySymbol;
                    waluta.Nazwa = ri.CurrencyEnglishName;
                    waluta.LokalnaNazwa = ri.CurrencyNativeName;
                    waluta.LokalnySymbol = ri.CurrencySymbol;
                }

                var kraj = ObjectSpace.FindObject<Kraj>(new BinaryOperator("Symbol", ri.ThreeLetterISORegionName));
                if (kraj == null)
                {
                    kraj = ObjectSpace.CreateObject<Kraj>();
                    kraj.Symbol = ri.ThreeLetterISORegionName;
                    kraj.Nazwa = ri.EnglishName;
                    kraj.LokalnySymbol = ri.TwoLetterISORegionName;
                    kraj.LokalnaNazwa = ri.NativeName;
                    kraj.GeoId = ri.GeoId;
                    kraj.Waluta = waluta;
                    kraj.IsMetric = ri.IsMetric;

                }
                waluta.Kraj = kraj;
            }
        }
    }
}
