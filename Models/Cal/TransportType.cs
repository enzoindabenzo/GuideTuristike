using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DK1.Models.Cal
{
        public enum TransportType
        {
            Taxi,
            Rental,
            Bus
        }

        public static class TransportRates
        {
            // Transport rates in Albanian Lek per kilometer
            public static double GetRatePerKm(TransportType transportType)
            {
                switch (transportType)
                {
                    case TransportType.Taxi:
                        return 300.0; // 150 Lek per km for taxi
                    case TransportType.Rental:
                        return 100.0; // 100 Lek per km for rental (plus base fee)
                    case TransportType.Bus:
                        return 15.0;  // 30 Lek per km for bus
                    default:
                        return 0.0;
                }
            }

            public static double GetBaseFee(TransportType transportType)
            {
                switch (transportType)
                {
                    case TransportType.Taxi:
                        return 300.0; // 300 Lek base fee for taxi
                    case TransportType.Rental:
                        return 5000.0; // 5000 Lek base fee for rental
                    case TransportType.Bus:
                        return 0.0;  // 100 Lek base fee for bus
                    default:
                        return 0.0;
                }
            }
        }
}