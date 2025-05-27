namespace DK1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class paypal : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Reservations", "TotalCost", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.Reservations", "PaymentMethod", c => c.String(nullable: false, maxLength: 50));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Reservations", "PaymentMethod");
            DropColumn("dbo.Reservations", "TotalCost");
        }
    }
}
