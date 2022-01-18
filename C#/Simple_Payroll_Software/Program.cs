using System;
using System.IO;
using static System.Console;
using System.Linq;

namespace SoftwareProgram
{
    class Program
    {
        static void Main(String[] args)
        {
            List<Staff> people = new List<Staff>{};
            FileReader fr = new FileReader();
            int year = 0;
            int month = 0;
            while(year == 0)
            {
                Write("\nPlease enter the year: ");
                try
                {
                    year = Convert.ToInt32(Console.ReadLine());
                }
                catch(FormatException)
                {
                    WriteLine("Please enter a valid integer year value");
                }
            }
            while(month == 0)
            {
                Write("\nPlease enter the month: ");
                try
                {
                    month = Convert.ToInt32(Console.ReadLine());
                    if(!(month >0 && month < 13))
                    {
                        WriteLine("Please enter a valid integer month value");
                        month = 0;
                    }
                }
                catch(FormatException)
                {
                    WriteLine("Please enter a valid integer month value");
                }
            }
            people = fr.ReadFile();
            for(int i = 0; i < people.Count; i++)
            {
                try
                {
                    WriteLine("Enter hours worked for {0}",people[i].NameOfStaff);
                    people[i].HoursWorked = Convert.ToInt32(Console.ReadLine());
                    people[i].CalculatePay();
                    Console.WriteLine(people[i].ToString());
                }
                catch(Exception e)
                {
                    WriteLine("{0}",e);
                    i--;
                }
            }

            PaySlip ps = new PaySlip(month,year);
            ps.GeneratePaySlip(people);
            ps.GenerateSummary(people);

        }

    }
    class Staff
    {
        private float hourlyRate;
        private int hWorked;

        public Staff(string name, float rate)
        {
            NameOfStaff = name;
            hourlyRate = rate;
        }

        public float TotalPay{get; protected set;}
        public float BasicPay{get; private set;}
        public string NameOfStaff{get;set;}
        public int HoursWorked
        {
            get
            {
                return hWorked;
            }
            set
            {
                if(value > 0)
                    hWorked = value;
                else
                    hWorked = 0;
            }
        }

        public virtual void CalculatePay()
        {
            WriteLine("Calculating Pay...");
            BasicPay = hWorked * hourlyRate;
            TotalPay = BasicPay;
        }

        public override string ToString(){
            return "Summary of " + NameOfStaff + "\n" +
            "h/r " + hourlyRate + " h/w " + hWorked + "\n" +
            "Total " + TotalPay + ", Basic " + BasicPay + "\n";
        }
    } 

    class Manager:Staff
    {
        private const float managerHourlyRate = 50;

        public int Allowance{private set;get;}

        public Manager(string name):base(name,managerHourlyRate){}

        public override void CalculatePay()
        {
            base.CalculatePay();
            Allowance = 1000;
            if(HoursWorked > 160)
            {
                TotalPay += Allowance;
            }
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }

    class Admin:Staff
    {
        private const float overtimeRate = 15.5F;
        private const float adminHourlyRate = 30;

        public float Overtime{private set; get;}

        public Admin(string name):base(name,adminHourlyRate){}

        public override void CalculatePay()
        {
            base.CalculatePay();
            if(HoursWorked > 160)
            {
                Overtime = overtimeRate * (HoursWorked-160);
                TotalPay += Overtime;
            }
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }

    class FileReader
    {
        public List<Staff> ReadFile()
        {
            List<Staff> staff = new List<Staff>();
            string[] result = new string[2];
            string path = "staff.txt";
            string[] separator = {", "};

            if (File.Exists(path))
            {
                using(StreamReader reader = new StreamReader(path))
                {
                    while (!reader.EndOfStream)
                    {
                        try{
                            var line = reader.ReadLine();
                            if(line != null)
                            {
                                string[] line_parts = line.Split(separator,StringSplitOptions.RemoveEmptyEntries);
                                if(line_parts[1] == "Manager")
                                    staff.Add(new Manager(line_parts[0]));
                                else if(line_parts[1] == "Admin")
                                    staff.Add(new Admin(line_parts[0]));
                            }
                        }
                        catch
                        {
                            break;
                        }
                    }
                    reader.Close();
                }
            }
            return staff;
        }
    }

    class PaySlip
    {
        private int month;
        private int year;

        enum MonthsOfYear
        {
            JAN = 1, FEB = 2, MAR = 3, APR = 4, MAY = 5, JUN = 6, 
            JUL = 7, AUG = 8, SEP = 9, OCT = 10, NOV = 11, DEC = 12
        }

        public PaySlip(int payMonth, int payYear)
        {
            month = payMonth;
            year = payYear;
        }

        public void GeneratePaySlip(List<Staff> staff)
        {
            foreach(Staff person in staff)
            {
                string path = person.NameOfStaff+".txt";
                using(StreamWriter writer = new StreamWriter(path))
                {
                    writer.WriteLine("PAYSLIP FOR {0} {1}",(MonthsOfYear) month,year);
                    writer.WriteLine("===========================");
                    writer.WriteLine("Name of Staff: {0}",person.NameOfStaff);
                    writer.WriteLine("Hours Worked: {0}",person.HoursWorked);
                    writer.WriteLine("");
                    writer.WriteLine("Basic Pay: {0:C}",person.BasicPay);
                    if(person.GetType() == typeof(Manager))
                    {
                        Manager manager_person = (Manager)person;
                        writer.WriteLine("Allowance: {0:C}",manager_person.Allowance);
                    }
                    else if(person.GetType() == typeof(Admin))
                    {
                        Admin admin_person = (Admin)person;
                        writer.WriteLine("Overtime Pay: {0:C}",admin_person.Overtime);
                    }
                    writer.WriteLine("");
                    writer.WriteLine("===========================");
                    writer.WriteLine("Total Pay: {0:C}",person.TotalPay);
                    writer.WriteLine("===========================");
                    writer.Close();
                }
            }
        }
    
        public void GenerateSummary(List<Staff> staff)
        {
            var result = from person in staff 
                                where person.HoursWorked < 10 
                                orderby person.NameOfStaff ascending
                                select new {person.NameOfStaff,person.HoursWorked};
            string path = "summary.txt";
            StreamWriter writer = new StreamWriter(path);
            writer.WriteLine("Staff with less than 10 working hours.");
            foreach(var person in result)
            {
                WriteLine("Name of Staff: {0}, Hours Worked: {1}",person.NameOfStaff,person.HoursWorked);
            }
            writer.Close();
        }

        public override string ToString()
        {
            return "Hello";
        }

    }
}

