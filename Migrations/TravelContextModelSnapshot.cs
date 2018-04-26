﻿// <auto-generated />
using System;
using Hitchhikers.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Hitchhikers.Migrations
{
    [DbContext(typeof(TravelContext))]
    partial class TravelContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.0-preview2-30571");

            modelBuilder.Entity("Hitchhikers.Models.Comment", b =>
                {
                    b.Property<int>("Commentid")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("CommentText");

                    b.Property<DateTime>("Created_At");

                    b.Property<int>("PictureId");

                    b.Property<int>("SenderId");

                    b.Property<DateTime>("Updated_At");

                    b.HasKey("Commentid");

                    b.HasIndex("PictureId");

                    b.HasIndex("SenderId");

                    b.ToTable("Comments");
                });

            modelBuilder.Entity("Hitchhikers.Models.Picture", b =>
                {
                    b.Property<int>("PictureId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Address");

                    b.Property<string>("City");

                    b.Property<DateTime>("Created_At");

                    b.Property<DateTime>("DateVisited");

                    b.Property<string>("Description");

                    b.Property<string>("PictName");

                    b.Property<string>("States");

                    b.Property<DateTime>("Updated_At");

                    b.Property<int>("UploaderId");

                    b.HasKey("PictureId");

                    b.HasIndex("UploaderId");

                    b.ToTable("Pictures");
                });

            modelBuilder.Entity("Hitchhikers.Models.User", b =>
                {
                    b.Property<int>("Userid")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("Created_At");

                    b.Property<string>("Email");

                    b.Property<string>("FirstName");

                    b.Property<string>("LastName");

                    b.Property<string>("Nickname");

                    b.Property<string>("Password");

                    b.Property<string>("ProfilePict");

                    b.Property<DateTime>("Updated_At");

                    b.HasKey("Userid");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Hitchhikers.Models.Comment", b =>
                {
                    b.HasOne("Hitchhikers.Models.Picture", "Picture")
                        .WithMany("PictComments")
                        .HasForeignKey("PictureId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Hitchhikers.Models.User", "Sender")
                        .WithMany("MyComments")
                        .HasForeignKey("SenderId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Hitchhikers.Models.Picture", b =>
                {
                    b.HasOne("Hitchhikers.Models.User", "Uploader")
                        .WithMany("Uploaded")
                        .HasForeignKey("UploaderId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
