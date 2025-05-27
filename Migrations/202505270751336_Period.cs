namespace DK1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Period : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Reservations", "Phone", c => c.String(maxLength: 20));
            AddColumn("dbo.Reservations", "NumberOfDays", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Reservations", "NumberOfDays");
            DropColumn("dbo.Reservations", "Phone");
        }
    }
}
