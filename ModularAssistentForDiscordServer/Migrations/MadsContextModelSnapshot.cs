﻿// <auto-generated />
using System;
using MADS.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace MADS.Migrations
{
    [DbContext(typeof(MadsContext))]
    partial class MadsContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("MADS.Entities.GuildConfigDbEntity", b =>
                {
                    b.Property<ulong>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint unsigned")
                        .HasColumnName("id");

                    b.Property<ulong>("DiscordGuildId")
                        .HasColumnType("bigint unsigned")
                        .HasColumnName("discordId");

                    b.Property<string>("Prefix")
                        .HasColumnType("longtext")
                        .HasColumnName("prefix");

                    b.Property<bool>("StarboardActive")
                        .HasColumnType("tinyint(1)")
                        .HasColumnName("starboardEnabled");

                    b.Property<ulong?>("StarboardChannelId")
                        .HasColumnType("bigint unsigned")
                        .HasColumnName("starboardChannel");

                    b.Property<string>("StarboardEmojiId")
                        .HasColumnType("longtext")
                        .HasColumnName("starboardEmojiId");

                    b.Property<int?>("StarboardThreshold")
                        .HasColumnType("int")
                        .HasColumnName("starboardThreshold");

                    b.HasKey("Id");

                    b.ToTable("GuildConfigDbEntity");
                });

            modelBuilder.Entity("MADS.Entities.GuildDbEntity", b =>
                {
                    b.Property<ulong>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint unsigned")
                        .HasColumnName("id");

                    b.Property<ulong>("DiscordId")
                        .HasColumnType("bigint unsigned")
                        .HasColumnName("discordId");

                    b.HasKey("Id");

                    b.ToTable("Guilds");
                });

            modelBuilder.Entity("MADS.Entities.GuildUserDbEntity", b =>
                {
                    b.Property<ulong>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint unsigned")
                        .HasColumnName("id");

                    b.Property<ulong>("DiscordId")
                        .HasColumnType("bigint unsigned")
                        .HasColumnName("discordId");

                    b.Property<ulong>("GuildId")
                        .HasColumnType("bigint unsigned")
                        .HasColumnName("guildId");

                    b.Property<ulong?>("UserId")
                        .HasColumnType("bigint unsigned");

                    b.HasKey("Id");

                    b.HasIndex("GuildId");

                    b.HasIndex("UserId");

                    b.ToTable("GuildUsers");
                });

            modelBuilder.Entity("MADS.Entities.IncidentDbEntity", b =>
                {
                    b.Property<ulong>("Id")
                        .HasColumnType("bigint unsigned");

                    b.Property<DateTimeOffset>("CreationTimeStamp")
                        .HasColumnType("datetime(6)");

                    b.Property<ulong>("ModeratorId")
                        .HasColumnType("bigint unsigned")
                        .HasColumnName("moderator_id");

                    b.Property<string>("Reason")
                        .HasColumnType("longtext")
                        .HasColumnName("reason");

                    b.Property<ulong>("TargetId")
                        .HasColumnType("bigint unsigned")
                        .HasColumnName("target_id");

                    b.Property<ulong?>("TargetUserId")
                        .HasColumnType("bigint unsigned");

                    b.HasKey("Id");

                    b.HasIndex("TargetUserId");

                    b.ToTable("Incidents");
                });

            modelBuilder.Entity("MADS.Entities.StarboardMessageDbEntity", b =>
                {
                    b.Property<ulong>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint unsigned")
                        .HasColumnName("id");

                    b.Property<ulong>("DiscordChannelId")
                        .HasColumnType("bigint unsigned")
                        .HasColumnName("discordChannelId");

                    b.Property<ulong>("DiscordGuildId")
                        .HasColumnType("bigint unsigned")
                        .HasColumnName("discordGuildId");

                    b.Property<ulong>("DiscordMessageId")
                        .HasColumnType("bigint unsigned")
                        .HasColumnName("discordMessageId");

                    b.Property<ulong>("StarboardChannelId")
                        .HasColumnType("bigint unsigned")
                        .HasColumnName("starboardChannelId");

                    b.Property<ulong>("StarboardGuildId")
                        .HasColumnType("bigint unsigned")
                        .HasColumnName("starboardGuildId");

                    b.Property<ulong>("StarboardMessageId")
                        .HasColumnType("bigint unsigned")
                        .HasColumnName("starboardMessageId");

                    b.Property<int>("Stars")
                        .HasColumnType("int")
                        .HasColumnName("starCount");

                    b.HasKey("Id");

                    b.ToTable("Starboard");
                });

            modelBuilder.Entity("MADS.Entities.UserDbEntity", b =>
                {
                    b.Property<ulong>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint unsigned")
                        .HasColumnName("id");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("MADS.Entities.GuildDbEntity", b =>
                {
                    b.HasOne("MADS.Entities.GuildConfigDbEntity", "Settings")
                        .WithOne("Guild")
                        .HasForeignKey("MADS.Entities.GuildDbEntity", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Settings");
                });

            modelBuilder.Entity("MADS.Entities.GuildUserDbEntity", b =>
                {
                    b.HasOne("MADS.Entities.GuildDbEntity", "Guild")
                        .WithMany("Users")
                        .HasForeignKey("GuildId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MADS.Entities.UserDbEntity", "User")
                        .WithMany("Guilds")
                        .HasForeignKey("UserId");

                    b.Navigation("Guild");

                    b.Navigation("User");
                });

            modelBuilder.Entity("MADS.Entities.IncidentDbEntity", b =>
                {
                    b.HasOne("MADS.Entities.GuildDbEntity", "Guild")
                        .WithMany("Incidents")
                        .HasForeignKey("Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MADS.Entities.UserDbEntity", "TargetUser")
                        .WithMany("Incidents")
                        .HasForeignKey("TargetUserId");

                    b.Navigation("Guild");

                    b.Navigation("TargetUser");
                });

            modelBuilder.Entity("MADS.Entities.GuildConfigDbEntity", b =>
                {
                    b.Navigation("Guild");
                });

            modelBuilder.Entity("MADS.Entities.GuildDbEntity", b =>
                {
                    b.Navigation("Incidents");

                    b.Navigation("Users");
                });

            modelBuilder.Entity("MADS.Entities.UserDbEntity", b =>
                {
                    b.Navigation("Guilds");

                    b.Navigation("Incidents");
                });
#pragma warning restore 612, 618
        }
    }
}
