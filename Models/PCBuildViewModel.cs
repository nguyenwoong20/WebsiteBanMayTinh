namespace Website_BanMayTinh.Models
{
    public class PCBuildViewModel
    {
        public List<Product> CPUs { get; set; }
        public List<Product> Mainboards { get; set; }
        public List<Product> RAMs { get; set; }
        public List<Product> SSDs { get; set; }
        public List<Product> HDDs { get; set; }
        public List<Product> VGAs { get; set; }
        public List<Product> PSUs { get; set; }
        public List<Product> Cases { get; set; }
        public List<Product> AirCoolers { get; set; }
        public List<Product> LiquidCoolers { get; set; }

        public int SelectedCPUId { get; set; }
        public int SelectedMainboardId { get; set; }
        public int SelectedRAMId { get; set; }
        public int SelectedSSDId { get; set; }
        public int SelectedHDDId { get; set; }
        public int SelectedVGAId { get; set; }
        public int SelectedPSUId { get; set; }
        public int SelectedCaseId { get; set; }
        public int SelectedAirCoolerId { get; set; }
        public int SelectedLiquidCoolerId { get; set; }

        public int Quantity_CPU { get; set; }
        public int Quantity_Mainboard { get; set; }
        public int Quantity_RAM { get; set; }
        public int Quantity_SSD { get; set; }
        public int Quantity_HDD { get; set; }
        public int Quantity_VGA { get; set; }
        public int Quantity_PSU { get; set; }
        public int Quantity_Case { get; set; }
        public int Quantity_AirCooler { get; set; }
        public int Quantity_LiquidCooler { get; set; }



    }
}
