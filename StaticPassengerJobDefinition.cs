﻿using DV.Logic.Job;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PassengerJobsMod
{
    class StaticPassengerJobDefinition : StaticJobDefinition
    {
        public List<Car> trainCarsToTransport;
        public Track startingTrack;
        public Track[] destinationTracks;
        public string[] destinationYards;

        public override JobDefinitionDataBase GetJobDefinitionSaveData()
        {
            return new PassengerJobDefinitionData(
                timeLimitForJob, initialWage,
                logicStation.ID, chainData.chainOriginYardId, chainData.chainDestinationYardId,
                (int)requiredLicenses, trainCarsToTransport.Select(car => car.carGuid).ToArray(),
                startingTrack.ID.FullID, destinationTracks.Select(track => track.ID.FullID).ToArray(),
                destinationYards);
        }

        public override List<TrackReservation> GetRequiredTrackReservations()
        {
            float reserveLength = YardTracksOrganizer.Instance.GetTotalCarsLength(trainCarsToTransport) + 
                YardTracksOrganizer.Instance.GetSeparationLengthBetweenCars(trainCarsToTransport.Count);

            return destinationTracks.Select(track => new TrackReservation(track, reserveLength)).ToList();
        }

        protected override void GenerateJob( Station jobOriginStation, float timeLimit = 0, float initialWage = 0, string forcedJobId = null, JobLicenses requiredLicenses = JobLicenses.Basic )
        {
            if( (trainCarsToTransport == null) || (trainCarsToTransport.Count == 0) ||
                (startingTrack == null) || (destinationTracks == null) || (destinationYards == null))
            {
                trainCarsToTransport = null;
                startingTrack = null;
                destinationTracks = null;
                destinationYards = null;
            }

            // Force cargo state
            foreach( var car in trainCarsToTransport )
            {
                car.DumpCargo();
                car.LoadCargo(car.capacity, CargoType.Passengers);
            }

            // Initialize tasks
            var tasks = new List<Task>();
            for( int i = 0; i < destinationTracks.Length; i++ )
            {
                tasks.Add(JobsGenerator.CreateTransportTask(trainCarsToTransport, destinationTracks[i], (i == 0) ? startingTrack : destinationTracks[i - 1], null));
            }

            job = new Job(tasks, PassengerJobGenerator.JT_Passenger, timeLimit, initialWage, chainData, forcedJobId, requiredLicenses);
            jobOriginStation.AddJobToStation(job);
        }
    }

    class PassengerJobDefinitionData : JobDefinitionDataBase
    {
        public PassengerJobDefinitionData( float timeLimitForJob, float initialWage, string stationId, string originStationId, string destinationStationId, int requiredLicenses, string[] transportCarGuids, string startTrackId, string[] destinationTrackIds, string[] destinationYardIds ) : 
            base(timeLimitForJob, initialWage, stationId, originStationId, destinationStationId, requiredLicenses)
        {
            trainCarGuids = transportCarGuids;
            startingTrack = startTrackId;
            destinationTracks = destinationTrackIds;
            destinationYards = destinationYardIds;
        }

        public string[] trainCarGuids;
        public string startingTrack;
        public string[] destinationTracks;
        public string[] destinationYards;
    }

    [Serializable]
    class ComplexChainData : StationsChainData
    {
        public string[] chainDestinationYardIds;

        public ComplexChainData( string originYardId, string[] destYardIds )
            : base(originYardId, destYardIds.Last())
        {
            chainDestinationYardIds = destYardIds;
        }
    }
}
