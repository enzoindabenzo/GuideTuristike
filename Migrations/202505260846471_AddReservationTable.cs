namespace DK1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddReservationTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Reservations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DestinationId = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 100),
                        Email = c.String(nullable: false, maxLength: 100),
                        ReservationDate = c.DateTime(nullable: false),
                        NumberOfPeople = c.Int(nullable: false),
                        Notes = c.String(maxLength: 500),
                        Status = c.String(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Destinations", t => t.DestinationId, cascadeDelete: true)
                .Index(t => t.DestinationId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Reservations", "DestinationId", "dbo.Destinations");
            DropIndex("dbo.Reservations", new[] { "DestinationId" });
            DropTable("dbo.Reservations");
        }
    }
}
