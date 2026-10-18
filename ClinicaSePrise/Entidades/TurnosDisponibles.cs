namespace ClinicaSePrise.Entidades
{
    public class Slot
    {
        public DateTime fechaHora { get; set; }
        public int duracion { get; set; }
        public string tipo { get; set; }

        public string hora => tipo == "Sobreturno" ? $"{fechaHora:HH:mm} · sobreturno" : fechaHora.ToString("HH:mm");

        public Slot(DateTime fechaHora, int duracion, string tipo)
        {
            this.fechaHora = fechaHora;
            this.duracion = duracion;
            this.tipo = tipo;
        }
    }
}