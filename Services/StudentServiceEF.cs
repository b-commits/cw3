﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using WebApplication1.EntityModels;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    class StudentServiceEF : IStudentsDbServiceEF
    {
        public void DeleteStudent(string ska)
        {
            s19677Context db = new s19677Context();
            EntityModels.Student student = db.Student.SingleOrDefault(s => s.IndexNumber == ska);
            db.Student.Remove(student);
            db.SaveChanges();
        }

        public void EnrollStudent(Student student)
        {
            s19677Context db = new s19677Context();
            // Sprawdź, czy przekazane wszystkie dane. Czy istnieją studia zgodne z wartością. 
            if (!(student.FirstName == null) && !(student.LastName == null) && !(student.IndexNumber == null) && !(student.BirthDate == null) && !(student.Studies == null)) {
                var studies = db.Studies.Select(s => s.Name == student.Studies);
                if (studies != null)
                {
                    var study = db.Studies.FirstOrDefault(s => s.Name == student.Studies);
                    var enrol = db.Enrollment.FirstOrDefault(e => e.Semester == 1 && e.IdStudy == study.IdStudy);
                    if (!db.Enrollment.Contains(enrol))                    
                    {
                        db.Enrollment.Add(new EntityModels.Enrollment { IdEnrollment = 99, Semester = 1, IdStudy = study.IdStudy, StartDate = DateTime.Now });
                        db.SaveChanges();
                    }
                    EntityModels.Student res = new EntityModels.Student
                    {
                        IndexNumber = student.IndexNumber,
                        FirstName = student.FirstName,
                        LastName = student.LastName,
                        BirthDate = student.BirthDate,
                        IdEnrollment = enrol.IdEnrollment
                    };
                    db.Add(res);
                    db.SaveChanges();
                }
            }
        }

        public List<EntityModels.Student> GetStudents()
        {
            s19677Context db = new s19677Context();
            var res = db.Student.ToList();
            return res;
        }

        public void PromoteStudents(StudiesSemester studiesSemester)
        {
            s19677Context db = new s19677Context();
            //W bazie mam już procedurę promoteStudents, więc można ją uruchomić w taki sposób:
             
            //  db.Database.ExecuteSqlRaw("EXEC promoteStudents");

            //lub wykonać instrukcję UPDATE ręcznie:

            var study = db.Studies.FirstOrDefault(s => s.Name == studiesSemester.Studies);
            var enrol = db.Enrollment.FirstOrDefault(e => e.IdStudy == study.IdStudy && e.Semester == studiesSemester.Semester);
            if (db.Enrollment.Contains(enrol))
            {
                Boolean existsEnrol2 = db.Enrollment.Any(e => e.IdStudy == study.IdStudy && e.Semester == studiesSemester.Semester + 1);
                var en0 = db.Enrollment.FirstOrDefault(e => e.IdStudy == study.IdStudy && e.Semester == studiesSemester.Semester + 1);
                int id;
                if (!existsEnrol2)
                {
                    var en = new EntityModels.Enrollment
                    {
                        IdEnrollment = 99,
                        Semester = studiesSemester.Semester + 1,
                        IdStudy = study.IdStudy,
                        StartDate = DateTime.Now
                    };
                    id = en.IdEnrollment;
                    db.Add(en);
                    db.SaveChanges();
                } else
                {
                    id = en0.IdEnrollment;
                }
                foreach (var s in db.Student.Where(s => s.IdEnrollment == enrol.IdEnrollment))
                {
                    s.IdEnrollment = id;
                }
                db.SaveChanges();
            }
        }

        public void UpdateStudent(EntityModels.Student student)
        {
            s19677Context db = new s19677Context();
            var res = new EntityModels.Student
            {
                IndexNumber = student.IndexNumber,
                FirstName = student.FirstName,
                LastName = student.LastName,
                BirthDate = student.BirthDate
            };
            db.Attach(res); // res w systemie śledzenia zmian.
            db.Entry(res).Property("FirstName").IsModified = true;
            db.Entry(res).Property("LastName").IsModified = true;
            db.Entry(res).Property("BirthDate").IsModified = true;

            db.SaveChanges();
        }

    }
}
