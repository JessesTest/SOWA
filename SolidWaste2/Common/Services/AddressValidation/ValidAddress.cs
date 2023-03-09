namespace Common.Services.AddressValidation
{
    public class ValidAddress
    {
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }

        public double Score { get; set; }

        public ValidAddress() { }

        public ValidAddress(string address, double score)
        {
            _ = address ?? throw new ArgumentNullException(nameof(address));
            var parts = address.Split(',');
            if (parts.Length != 4)
                throw new ArgumentException("Invalid address", nameof(address));

            Address = parts[0].Trim();
            City = parts[1].Trim();
            State = parts[2].Trim();
            Zip = parts[3].Trim();

            Score = score;
        }

        #region object overrides

        public override string ToString()
        {
            return $"{Address}, {City}, {State}, {Zip}";
        }

        public override bool Equals(object obj)
        {
            return obj is ValidAddress other
                && Address == other.Address
                && City == other.City
                && State == other.State
                && Zip == other.Zip;
        }

        public override int GetHashCode()
        {
            HashCode hash = new();
            hash.Add(Address);
            hash.Add(City);
            hash.Add(State);
            hash.Add(Zip);
            return hash.ToHashCode();
        }

        #endregion
    }
}
