namespace ClinicaSePrise.Entidades
{
    public class ComprobanteEstudio
    {
        public DateTime fechaAtencion { get; set; }
        public string nombre { get; set; }
        public bool retiro { get; set; }

        public string fecha => fechaAtencion.ToString("dd/MM/yyyy HH:mm");
        public string estado => retiro ? "Retirado" : "Pendiente de retiro";

        public ComprobanteEstudio(DateTime fechaAtencion, string nombre, bool retiro)
        {
            this.fechaAtencion = fechaAtencion;
            this.nombre = nombre;
            this.retiro = retiro;
        }
    }
}