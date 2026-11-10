namespace UI;

using System.Globalization;

public static class MoneyFormatting
{
  // Prices are stored in minor units (cents). Display them as euros with two decimals.
  public static string ToEuro(this decimal cents) =>
    (cents / 100m).ToString("N2", CultureInfo.InvariantCulture) + " EUR";
}
