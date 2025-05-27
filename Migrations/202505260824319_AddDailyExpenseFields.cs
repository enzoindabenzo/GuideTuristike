namespace DK1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddDailyExpenseFields : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Destinations", "MinDailyExpenseAL", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.Destinations", "MaxDailyExpenseAL", c => c.Decimal(precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Destinations", "MaxDailyExpenseAL");
            DropColumn("dbo.Destinations", "MinDailyExpenseAL");
        }
    }
}
