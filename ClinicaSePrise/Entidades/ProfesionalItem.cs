namespace ClinicaSePrise.Entidades
{
    public class ProfesionalItem
    {
        public int idProfesional { get; set; }
        public string nombre { get; set; }

        public ProfesionalItem(int idProfesional, string nombre)
        {
            this.idProfesional = idProfesional;
            this.nombre = nombre;
        }
    }
}