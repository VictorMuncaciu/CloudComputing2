using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Grpc.Core;
using ZodiacFinderGrpc;

namespace ZodiacFinder
{
    class ZodiacFinderImpl : ZodiacService.ZodiacServiceBase
    {
        
        private static string ZodiacFile = ".."+Path.DirectorySeparatorChar+".."+Path.DirectorySeparatorChar+".."+Path.DirectorySeparatorChar+"zodiac.txt";

        public override Task<ZodiacReply> findZodie(BirthDateRequest request, ServerCallContext context)
        {
            Console.WriteLine("Request: "+request.Day+"/"+request.Mo+"/"+request.Year);

            Task<ZodiacReply> reply=null;

            try
            {
                reply = Task.FromResult(FindSignForDate(request));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            
            return reply;
        }

        //Citeste fisierul de zodii
        public static SignRange[] ReadSignFile()
        {
            var fileContent = System.IO.File.ReadAllText(ZodiacFile);

            //array cu toate liniile din fisierul de zodii
            var lines = fileContent.Split("\n");

            //array cu obiecte care reprezinta intervalul fiecarei zodii
            var signRanges = new SignRange[12];
            for (var i=0; i<lines.Length; i++)
            {
                //campurile prin care intervalul zodiei este reprezentat
                var fields = lines[i].Split(";");
                var start = fields[0];
                var end = fields[1];

                //indexul zodiei din fiseriul .proto
                var signIndex = Int32.Parse(fields[2]);

                var startDay = Int32.Parse(start.Split("/")[0]);
                var startMonth = Int32.Parse(start.Split("/")[1]);
                var endDay = Int32.Parse(end.Split("/")[0]);
                var endMonth = Int32.Parse(end.Split("/")[1]);

                //creem obiect cu toate proprietetiile unui interval de zodie
                var signRange=new SignRange{StartDay = startDay, StartMonth = startMonth, EndDay = endDay, EndMonth = endMonth, SignIndex = signIndex};
                
                signRanges[i] = signRange;
            }

            return signRanges;
        }

        private ZodiacReply FindSignForDate(BirthDateRequest birthDate)
        {
            var day = birthDate.Day;
            var month = birthDate.Mo;
            var year = birthDate.Year;
            
            var season = Seasons.Spring;
            var sign = ZodiacSign.Aquarius;

            var signRanges = ReadSignFile();

            int signIndex = -1;
            foreach (var signRange in signRanges)
            {
                if ((month == signRange.StartMonth && day>=signRange.StartDay) || (month == signRange.EndMonth && day<=signRange.EndDay) )
                {
                    signIndex = signRange.SignIndex;
                    break;
                }
            }

            if (signIndex > -1)
            {
                if (month == 1 || month == 2 || month==12)
                {
                    season = Seasons.Winter;
                }else if (month == 3 || month == 4 || month == 5)
                {
                    season = Seasons.Spring;
                }else if (month == 6 || month == 7 || month == 8)
                {
                    season = Seasons.Summer;
                }else if (month == 9 || month == 10 || month == 11)
                {
                    season = Seasons.Fall;
                }
                
                return new ZodiacReply() {Season = season, Sign = (ZodiacSign)signIndex};
            }
            else
            {
                Console.WriteLine("Could not find sign");
                return null;
            }
        }
    }

    class Program
    {
        const int Port = 88888;

        public static void Main(string[] args)
        {
            Server server = new Server
            {
                Services = { ZodiacService.BindService(new ZodiacFinderImpl()) },
                Ports = { new ServerPort("localhost", Port, ServerCredentials.Insecure) }
            };
            server.Start();
            
            Console.WriteLine("Server listening on port " + Port);
            Console.WriteLine("Press any key to stop the server...");
            Console.ReadKey();
            
            server.ShutdownAsync().Wait();
        }
    }
}