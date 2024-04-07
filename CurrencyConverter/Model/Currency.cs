namespace CurrencyConverter.Model
{
    public record Currency (string Code, string Name)
    {
        public override string ToString() => $"{Code}\t{Name}";
    }
}
