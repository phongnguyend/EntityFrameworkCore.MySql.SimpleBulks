using System.ComponentModel.DataAnnotations;

namespace EntityFrameworkCore.MySql.SimpleBulks.ConnectionExtensionsTests.Database;

public class Customer
{
    public Guid Id { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    [MaxLength(100)]
    public string CurrentCountryIsoCode { get; set; }

    public int Index { get; set; }

    public Season? Season { get; set; }

    public Season? SeasonAsString { get; set; }

    public ICollection<Contact> Contacts { get; set; }
}
