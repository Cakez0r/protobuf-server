
namespace Data.Players
{
    public class PlayerModel
    {
        public int PlayerID { get; set; }
        public int AccountID { get; set; }
        public string Name { get; set; }
        public float Health { get; set; }
        public float Power { get; set; }
        public long Money { get; set; }
        public int Map { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public float Rotation { get; set; }
    }
}
