namespace HotelReservation.Models
{
    public class Cliente
    {
        public int ClienteId { get; set; }
        public string Nome { get; set; }
        public string Cognome { get; set; }
        public string Email { get; set; }
        public string Telefono { get; set; }

        public ICollection<Prenotazione> Prenotazioni { get; set; }
    }
}

