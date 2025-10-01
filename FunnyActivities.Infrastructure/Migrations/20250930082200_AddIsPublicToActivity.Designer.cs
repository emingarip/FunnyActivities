using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace FunnyActivities.Infrastructure.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20250930082200_AddIsPublicToActivity")]
    partial class AddIsPublicToActivity
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("FunnyActivities.Domain.Entities.Activity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("ActivityCategoryId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<string>("Duration_Hours")
                        .HasColumnType("text");

                    b.Property<string>("Duration_Minutes")
                        .HasColumnType("text");

                    b.Property<string>("Duration_Seconds")
                        .HasColumnType("text");

                    b.Property<bool>("IsPublic")
                        .HasColumnType("boolean");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("VideoUrl_Value")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("ActivityCategoryId");

                    b.ToTable("Activities");
                });

            modelBuilder.Entity("FunnyActivities.Domain.Entities.ActivityCategory", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.ToTable("ActivityCategories");
                });

            modelBuilder.Entity("FunnyActivities.Domain.Entities.ActivityProductVariant", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("ActivityId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<decimal>("Price")
                        .HasColumnType("numeric");

                    b.Property<Guid>("ProductVariantId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("ActivityId");

                    b.HasIndex("ProductVariantId");

                    b.ToTable("ActivityProductVariants");
                });

            modelBuilder.Entity("FunnyActivities.Domain.Entities.AuditLog", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Action")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Details")
                        .HasColumnType("text");

                    b.Property<string>("IPAddress")
                        .HasColumnType("text");

                    b.Property<Guid?>("UserId")
                        .HasColumnType("uuid");

                    b.Property<string>("UserAgent")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AuditLogs");
                });

            modelBuilder.Entity("FunnyActivities.Domain.Entities.BaseProduct", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.ToTable("BaseProducts");
                });

            modelBuilder.Entity("FunnyActivities.Domain.Entities.Category", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.ToTable("Categories");
                });

            modelBuilder.Entity("FunnyActivities.Domain.Entities.Favorites", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("ActivityId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("ActivityId");

                    b.HasIndex("UserId");

                    b.ToTable("Favorites");
                });

            modelBuilder.Entity("FunnyActivities.Domain.Entities.Image", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("FileName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("ObjectKey")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.ToTable("Images");
                });

            modelBuilder.Entity("FunnyActivities.Domain.Entities.ProductVariant", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("BaseProductId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<decimal>("Price")
                        .HasColumnType("numeric");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("BaseProductId");

                    b.ToTable("ProductVariants");
                });

            modelBuilder.Entity("FunnyActivities.Domain.Entities.Role", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Permissions")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.ToTable("Roles");
                });

            modelBuilder.Entity("FunnyActivities.Domain.Entities.ShoppingCartItem", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("ActivityProductVariantId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("Quantity")
                        .HasColumnType("integer");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("ActivityProductVariantId");

                    b.HasIndex("UserId");

                    b.ToTable("ShoppingCartItems");
                });

            modelBuilder.Entity("FunnyActivities.Domain.Entities.Step", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("ActivityId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int?>("DurationSeconds")
                        .HasColumnType("integer");

                    b.Property<string>("MediaAttachments")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Order")
                        .HasColumnType("integer");

                    b.Property<int?>("PauseTimeSeconds")
                        .HasColumnType("integer");

                    b.Property<int?>("TimestampSeconds")
                        .HasColumnType("integer");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("ActivityId");

                    b.ToTable("Steps");
                });

            modelBuilder.Entity("FunnyActivities.Domain.Entities.Survey", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime?>("EndDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("IsActive")
                        .HasColumnType("boolean");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<uint>("RowVersion")
                        .IsConcurrencyToken()
                        .HasColumnType("xid");

                    b.Property<string>("ShareToken")
                        .HasColumnType("text");

                    b.Property<DateTime?>("StartDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("ShareToken")
                        .IsUnique();

                    b.HasIndex("UserId");

                    b.ToTable("Surveys");
                });

            modelBuilder.Entity("FunnyActivities.Domain.Entities.SurveyActivity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("ActivityId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("SurveyId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("ActivityId");

                    b.HasIndex("SurveyId");

                    b.ToTable("SurveyActivities");
                });

            modelBuilder.Entity("FunnyActivities.Domain.Entities.SurveyParticipant", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CompletedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Email")
                        .HasColumnType("text");

                    b.Property<string>("FirstName")
                        .HasColumnType("text");

                    b.Property<string>("LastName")
                        .HasColumnType("text");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("text");

                    b.Property<Guid>("SurveyId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("SurveyId");

                    b.ToTable("SurveyParticipants");
                });

            modelBuilder.Entity("FunnyActivities.Domain.Entities.SurveyVote", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("ActivityId")
                        .HasColumnType("uuid");

                    b.Property<string>("Comment")
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("Rating")
                        .HasColumnType("integer");

                    b.Property<Guid>("SurveyParticipantId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("ActivityId");

                    b.HasIndex("SurveyParticipantId");

                    b.ToTable("SurveyVotes");
                });

            modelBuilder.Entity("FunnyActivities.Domain.Entities.UnitOfMeasure", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Symbol")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.ToTable("UnitOfMeasures");
                });

            modelBuilder.Entity("FunnyActivities.Domain.Entities.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("IsActive")
                        .HasColumnType("boolean");

                    b.Property<DateTime?>("LastLoginDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("text");

                    b.Property<Guid>("RoleId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("Email")
                        .IsUnique();

                    b.HasIndex("RoleId");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("FunnyActivities.Domain.Entities.Activity", b =>
                {
                    b.HasOne("FunnyActivities.Domain.Entities.ActivityCategory", "ActivityCategory")
                        .WithMany("Activities")
                        .HasForeignKey("ActivityCategoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ActivityCategory");
                });

            modelBuilder.Entity("FunnyActivities.Domain.Entities.ActivityCategory", b =>
                {
                    b.Navigation("Activities");
                });

            modelBuilder.Entity("FunnyActivities.Domain.Entities.ActivityProductVariant", b =>
                {
                    b.HasOne("FunnyActivities.Domain.Entities.Activity", "Activity")
                        .WithMany("ActivityProductVariants")
                        .HasForeignKey("ActivityId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("FunnyActivities.Domain.Entities.ProductVariant", "ProductVariant")
                        .WithMany("ActivityProductVariants")
                        .HasForeignKey("ProductVariantId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Activity");

                    b.Navigation("ProductVariant");
                });

            modelBuilder.Entity("FunnyActivities.Domain.Entities.Activity", b =>
                {
                    b.Navigation("ActivityProductVariants");
                });

            modelBuilder.Entity("FunnyActivities.Domain.Entities.ProductVariant", b =>
                {
                    b.Navigation("ActivityProductVariants");
                });

            modelBuilder.Entity("FunnyActivities.Domain.Entities.AuditLog", b =>
                {
                    b.HasOne("FunnyActivities.Domain.Entities.User", "User")
                        .WithMany("AuditLogs")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("User");
                });

            modelBuilder.Entity("FunnyActivities.Domain.Entities.User", b =>
                {
                    b.Navigation("AuditLogs");
                });

            modelBuilder.Entity("FunnyActivities.Domain.Entities.Favorites", b =>
                {
                    b.HasOne("FunnyActivities.Domain.Entities.Activity", "Activity")
                        .WithMany("Favorites")
                        .HasForeignKey("ActivityId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("FunnyActivities.Domain.Entities.User", "User")
                        .WithMany("Favorites")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Activity");

                    b.Navigation("User");
                });

            modelBuilder.Entity("FunnyActivities.Domain.Entities.Activity", b =>
                {
                    b.Navigation("Favorites");
                });

            modelBuilder.Entity("FunnyActivities.Domain.Entities.User", b =>
                {
                    b.Navigation("Favorites");
                });

            modelBuilder.Entity("FunnyActivities.Domain.Entities.ProductVariant", b =>
                {
                    b.HasOne("FunnyActivities.Domain.Entities.BaseProduct", "BaseProduct")
                        .WithMany("ProductVariants")
                        .HasForeignKey("BaseProductId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("BaseProduct");
                });

            modelBuilder.Entity("FunnyActivities.Domain.Entities.BaseProduct", b =>
                {
                    b.Navigation("ProductVariants");
                });

            modelBuilder.Entity("FunnyActivities.Domain.Entities.ShoppingCartItem", b =>
                {
                    b.HasOne("FunnyActivities.Domain.Entities.ActivityProductVariant", "ActivityProductVariant")
                        .WithMany("ShoppingCartItems")
                        .HasForeignKey("ActivityProductVariantId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("FunnyActivities.Domain.Entities.User", "User")
                        .WithMany("ShoppingCartItems")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ActivityProductVariant");

                    b.Navigation("User");
                });

            modelBuilder.Entity("FunnyActivities.Domain.Entities.ActivityProductVariant", b =>
                {
                    b.Navigation("ShoppingCartItems");
                });

            modelBuilder.Entity("FunnyActivities.Domain.Entities.User", b =>
                {
                    b.Navigation("ShoppingCartItems");
                });

            modelBuilder.Entity("FunnyActivities.Domain.Entities.Step", b =>
                {
                    b.HasOne("FunnyActivities.Domain.Entities.Activity", "Activity")
                        .WithMany("Steps")
                        .HasForeignKey("ActivityId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Activity");
                });

            modelBuilder.Entity("FunnyActivities.Domain.Entities.Activity", b =>
                {
                    b.Navigation("Steps");
                });

            modelBuilder.Entity("FunnyActivities.Domain.Entities.Survey", b =>
                {
                    b.HasOne("FunnyActivities.Domain.Entities.User", "User")
                        .WithMany("Surveys")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("FunnyActivities.Domain.Entities.User", b =>
                {
                    b.Navigation("Surveys");
                });

            modelBuilder.Entity("FunnyActivities.Domain.Entities.SurveyActivity", b =>
                {
                    b.HasOne("FunnyActivities.Domain.Entities.Activity", "Activity")
                        .WithMany("SurveyActivities")
                        .HasForeignKey("ActivityId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("FunnyActivities.Domain.Entities.Survey", "Survey")
                        .WithMany("SurveyActivities")
                        .HasForeignKey("SurveyId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Activity");

                    b.Navigation("Survey");
                });

            modelBuilder.Entity("FunnyActivities.Domain.Entities.Activity", b =>
                {
                    b.Navigation("SurveyActivities");
                });

            modelBuilder.Entity("FunnyActivities.Domain.Entities.Survey", b =>
                {
                    b.Navigation("SurveyActivities");
                });

            modelBuilder.Entity("FunnyActivities.Domain.Entities.SurveyParticipant", b =>
                {
                    b.HasOne("FunnyActivities.Domain.Entities.Survey", "Survey")
                        .WithMany("SurveyParticipants")
                        .HasForeignKey("SurveyId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Survey");
                });

            modelBuilder.Entity("FunnyActivities.Domain.Entities.Survey", b =>
                {
                    b.Navigation("SurveyParticipants");
                });

            modelBuilder.Entity("FunnyActivities.Domain.Entities.SurveyVote", b =>
                {
                    b.HasOne("FunnyActivities.Domain.Entities.Activity", "Activity")
                        .WithMany("SurveyVotes")
                        .HasForeignKey("ActivityId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("FunnyActivities.Domain.Entities.SurveyParticipant", "SurveyParticipant")
                        .WithMany("SurveyVotes")
                        .HasForeignKey("SurveyParticipantId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Activity");

                    b.Navigation("SurveyParticipant");
                });

            modelBuilder.Entity("FunnyActivities.Domain.Entities.Activity", b =>
                {
                    b.Navigation("SurveyVotes");
                });

            modelBuilder.Entity("FunnyActivities.Domain.Entities.SurveyParticipant", b =>
                {
                    b.Navigation("SurveyVotes");
                });

            modelBuilder.Entity("FunnyActivities.Domain.Entities.User", b =>
                {
                    b.HasOne("FunnyActivities.Domain.Entities.Role", "Role")
                        .WithMany("Users")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Role");
                });

            modelBuilder.Entity("FunnyActivities.Domain.Entities.Role", b =>
                {
                    b.Navigation("Users");
                });
#pragma warning restore 612, 618
        }
    }
}