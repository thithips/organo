namespace Estetica_Unicid.Models
{
    public class ClienteModel : BaseModel
    {
        public string Nome { get; set; }
        public string Telefone { get; set; }
        public string Email { get; set; }
        public List<AgendamentoModel> Agendamentos { get; set; }
    }
}