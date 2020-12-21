﻿using DV.Logic.Job;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PassengerJobsMod
{
    internal static class Extensions
    {
        internal static T ChooseOne<T>( this IList<T> list, Random rand, T toExclude = default )
        {
            if( list == null || list.Count == 0 ) return default;

            T result;

            do
            {
                int i = rand.Next(list.Count);
                result = list[i];
            }
            while( Equals(result, toExclude) );

            return result;
        }

        internal static List<T> ChooseMany<T>( this IList<T> source, Random rand, int count )
        {
            var result = new List<T>(count);

            for( int i = 0; i < count; i++ )
            {
                result.Add(ChooseOne(source, rand));
            }

            return result;
        }

        internal static CarGuidsPerTrackId GetIdData( this CarsPerTrack carsPerTrack )
        {
            return new CarGuidsPerTrackId(
                carsPerTrack.track.ID.FullID, 
                carsPerTrack.cars.Select(c => c.carGuid).ToArray());
        }

        internal static CarsPerTrack GetCarTracksByIds( this CarGuidsPerTrackId carsPerTrack )
        {
            if( !YardTracksOrganizer.Instance.yardTrackIdToTrack.TryGetValue(carsPerTrack.trackId, out Track track) )
            {
                throw new ArgumentException($"No Track corresponding to ID: {carsPerTrack.trackId}");
            }

            var cars = new List<Car>();
            foreach( string guid in carsPerTrack.carGuids )
            {
                if( !SingletonBehaviour<IdGenerator>.Instance.carGuidToCar.TryGetValue(guid, out Car car) )
                {
                    throw new ArgumentException($"No Car corresponding to GUID: {guid}");
                }
                cars.Add(car);
            }

            return new CarsPerTrack(track, cars);
        }

        internal static bool IsTrackReserved( this YardTracksOrganizer yto, Track track )
        {
            return yto.GetReservedSpace(track) > 40.5f;
        }
    }
}
