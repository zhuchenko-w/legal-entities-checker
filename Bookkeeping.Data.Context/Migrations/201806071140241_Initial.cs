namespace Bookkeeping.Data.Context.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "public.Agents",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Code = c.String(),
                        Password = c.String(),
                        LastSeen = c.DateTime(),
                        IsLocked = c.Boolean(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "public.Resolutions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Note = c.String(),
                        Decision = c.Boolean(),
                        PurposeOfPaymentComment = c.String(),
                        ImageData = c.String(),
                        ImageName = c.String(),
                        MimeType = c.String(),
                        DateResolution = c.DateTime(),
                        AgentId = c.Int(nullable: false),
                        TaskId = c.Int(nullable: false),
                        DateTimeDesktopUpdate = c.DateTime(nullable: false),
                        DateTimeMobileUpdate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("public.Agents", t => t.AgentId, cascadeDelete: true)
                .ForeignKey("public.Tasks", t => t.TaskId, cascadeDelete: true)
                .Index(t => t.AgentId)
                .Index(t => t.TaskId);
            
            CreateTable(
                "public.Tasks",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Inn = c.String(),
                        Name = c.String(),
                        PurposeOfPayment = c.String(),
                        Date = c.DateTime(nullable: false),
                        TotalCount = c.Int(nullable: false),
                        ClientId = c.Int(),
                        IsArchive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "public.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
            CreateTable(
                "public.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("public.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .ForeignKey("public.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "public.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "public.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("public.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "public.AspNetUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("public.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("public.AspNetUserRoles", "UserId", "public.AspNetUsers");
            DropForeignKey("public.AspNetUserLogins", "UserId", "public.AspNetUsers");
            DropForeignKey("public.AspNetUserClaims", "UserId", "public.AspNetUsers");
            DropForeignKey("public.AspNetUserRoles", "RoleId", "public.AspNetRoles");
            DropForeignKey("public.Resolutions", "TaskId", "public.Tasks");
            DropForeignKey("public.Resolutions", "AgentId", "public.Agents");
            DropIndex("public.AspNetUserLogins", new[] { "UserId" });
            DropIndex("public.AspNetUserClaims", new[] { "UserId" });
            DropIndex("public.AspNetUsers", "UserNameIndex");
            DropIndex("public.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("public.AspNetUserRoles", new[] { "UserId" });
            DropIndex("public.AspNetRoles", "RoleNameIndex");
            DropIndex("public.Resolutions", new[] { "TaskId" });
            DropIndex("public.Resolutions", new[] { "AgentId" });
            DropTable("public.AspNetUserLogins");
            DropTable("public.AspNetUserClaims");
            DropTable("public.AspNetUsers");
            DropTable("public.AspNetUserRoles");
            DropTable("public.AspNetRoles");
            DropTable("public.Tasks");
            DropTable("public.Resolutions");
            DropTable("public.Agents");
        }
    }
}
