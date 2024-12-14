-- Kinderen toevoegen
DELETE FROM Kinderen WHERE PersoonId IN (1, 2, 3, 4);
SET IDENTITY_INSERT Kinderen ON;
INSERT INTO Kinderen (Id, PersoonId, Voornaam, Naam, Geboortedatum, Allergieen, Medicatie) VALUES
(11, 1011, 'Emma', 'Doe', '2010-06-01', 'Noten', 'Epipen'),
(12, 1011, 'Tom', 'Doe', '2012-12-15', 'Geen', 'Geen'),
(13, 1011, 'Lisa', 'Smith', '2013-03-21', 'Pollen', 'Antihistamine'),
(14, 1011, 'Ben', 'Johnson', '2014-08-09', 'Geen', 'Geen');
SET IDENTITY_INSERT Kinderen OFF;

-- Bestemmingen
DELETE FROM Bestemmingen WHERE Id IN (1, 2, 3, 4);
SET IDENTITY_INSERT Bestemmingen ON;
INSERT INTO Bestemmingen (Id, Code, BestemmingsNaam, Beschrijving, MinLeeftijd, MaxLeeftijd) VALUES
(1, 'B001', 'Parijs', 'Een reis naar Parijs.', 10, 18),
(2, 'B002', 'Londen', 'Een culturele trip naar Londen.', 12, 18),
(3, 'B003', 'Rome', 'Een historische reis naar Rome.', 14, 18),
(4, 'B004', 'Barcelona', 'Zon, zee en cultuur in Barcelona.', 12, 18);
SET IDENTITY_INSERT Bestemmingen OFF;

-- Groepsreizen toevoegen
DELETE FROM Groepsreizen WHERE Id IN (1, 2, 3, 4, 5, 6, 7, 8);
SET IDENTITY_INSERT Groepsreizen ON;
INSERT INTO Groepsreizen (Id, BestemmingId, BeginDatum, EindDatum, Prijs, IsArchived) VALUES
(1, 1, '2024-05-01', '2024-05-07', 500.00, 1),
(2, 2, '2024-06-10', '2024-06-15', 600.00, 0),
(3, 3, '2024-07-01', '2024-07-08', 700.00, 0),
(4, 4, '2024-08-15', '2024-08-22', 650.00, 0),
(5, 1, '2025-05-01', '2025-05-07', 520.00, 0), -- Toekomstige reis Parijs
(6, 2, '2025-06-15', '2025-06-20', 630.00, 0), -- Toekomstige reis Londen
(7, 3, '2025-07-10', '2025-07-17', 720.00, 0), -- Toekomstige reis Rome
(8, 4, '2025-08-20', '2025-08-27', 670.00, 0); -- Toekomstige reis Barcelona
SET IDENTITY_INSERT Groepsreizen OFF;

-- Activiteiten toevoegen
DELETE FROM Activiteiten WHERE Id IN (1, 2, 3, 4, 5);
SET IDENTITY_INSERT Activiteiten ON;
INSERT INTO Activiteiten (Id, Naam, Beschrijving) VALUES
(1, 'Eiffeltoren Bezoek', 'Een bezoek aan de Eiffeltoren.'),
(2, 'Musea Bezoeken', 'Ontdek de rijke geschiedenis van Parijs.'),
(3, 'Londen Brug', 'Bezoek de iconische brug van Londen.'),
(4, 'Colosseum Tour', 'Bezoek aan het iconische Colosseum in Rome.'),
(5, 'Camp Nou Tour', 'Verken het beroemde stadion van Barcelona.');
SET IDENTITY_INSERT Activiteiten OFF;

-- Programma's koppelen aan groepsreizen
DELETE FROM Programmas WHERE GroepsreisId IN (1, 2, 3, 4);
INSERT INTO Programmas (Id, ActiviteitId, GroepsreisId) VALUES
(1, 1, 1), -- Activiteit 1 bij Parijs
(2, 2, 1), -- Activiteit 2 bij Parijs
(3, 3, 2), -- Activiteit 3 bij Londen
(4, 4, 3), -- Colosseum Tour bij Rome
(5, 5, 4), -- Camp Nou Tour bij Barcelona
(6, 1, 5), -- Activiteit 1 bij Parijs (2025)
(7, 3, 6), -- Activiteit 3 bij Londen (2025)
(8, 4, 7), -- Activiteit 4 bij Rome (2025)
(9, 5, 8); -- Activiteit 5 bij Barcelona (2025)

-- Onkosten toevoegen
DELETE FROM Onkosten WHERE Id IN (1, 2, 3, 4, 5, 6);
SET IDENTITY_INSERT Onkosten ON;
INSERT INTO Onkosten (Id, Titel, Omschrijving, Bedrag, Datum, Foto, GroepsreisId, TypeOnkost) VALUES
(1, 'Lunch in Parijs', 'Groepslunch tijdens reis.', 150.00, '2024-05-03', 'lunch.jpg', 1, 'Verantwoordelijke'),
(2, 'Treintickets', 'Reis naar Londen.', 300.00, '2024-06-11', NULL, 2, 'Hoofdmonitor'),
(3, 'Extra drankjes', 'Drankjes tijdens diner', 50.00, '2024-05-04', NULL, 1, 'Hoofdmonitor'),
(4, 'Souvenirs', 'Kleine cadeaus', 30.00, '2024-06-12', NULL, 2, 'Verantwoordelijke'),
(5, 'Museumtickets', 'Tickets voor musea in Rome', 120.00, '2024-07-03', 'museum.jpg', 3, 'Verantwoordelijke'),
(6, 'Stranddag', 'Toegang strandclub in Barcelona', 90.00, '2024-08-17', 'strand.jpg', 4, 'Verantwoordelijke'),
(7, 'Picknick in Parijs', 'Lunch op het gras bij de Eiffeltoren.', 160.00, '2025-05-03', 'picknick.jpg', 5, 'Verantwoordelijke'),
(8, 'Metrokaartjes', 'Reis binnen Londen.', 80.00, '2025-06-16', 'metro.jpg', 6, 'Verantwoordelijke'),
(9, 'Gidskosten', 'Professionele gids in Rome', 200.00, '2025-07-12', NULL, 7, 'Verantwoordelijke'),
(10, 'Paella-avond', 'Gezamenlijk diner in Barcelona', 140.00, '2025-08-21', 'paella.jpg', 8, 'Verantwoordelijke');
SET IDENTITY_INSERT Onkosten OFF;

-- Monitoren toevoegen
SET IDENTITY_INSERT Monitoren ON;
INSERT INTO Monitoren (Id, PersoonId, IsHoofdMonitor) VALUES
(11, 1012, 0), -- Monitor
(12, 1013, 1); -- Hoofdmonitor
SET IDENTITY_INSERT Monitoren OFF;

-- Monitoren koppelen aan groepsreizen
DELETE FROM GroepsreisMonitor WHERE GroepsreisId IN (1, 2, 3, 4);
INSERT INTO GroepsreisMonitor (GroepsreisId, MonitorId) VALUES
(1, 1), -- Monitor bij Parijs
(2, 2), -- Hoofdmonitor bij Londen
(3, 2), -- Monitor bij Rome
(4, 1); -- Monitor bij Barcelona

-- Deelnemers toevoegen
DELETE FROM Deelnemers WHERE Id IN (1, 2, 3, 4);
SET IDENTITY_INSERT Deelnemers ON;
INSERT INTO Deelnemers (Id, KindId, GroepsreisDetailId, Opmerkingen, ReviewScore, Review) VALUES
(1, 1, 1, 'Geweldige reis!', 5, 'Fantastische ervaring.'),
(2, 2, 2, 'Leuke reis!', 4, 'Goed georganiseerd.'),
(3, 3, 3, 'Leerzaam en leuk!', 5, 'Fantastische reiservaring.'),
(4, 4, 4, 'Geweldige locatie!', 4, 'Zeer goed georganiseerd.');
SET IDENTITY_INSERT Deelnemers OFF;

-- Opleidingen toevoegen
DELETE FROM Opleidingen WHERE Id IN (1, 2, 3, 4, 5, 6);
SET IDENTITY_INSERT Opleidingen ON;
INSERT INTO Opleidingen (Id, Naam, Beschrijving, BeginDatum, EindDatum, AantalPlaatsen, OpleidingVereistId) VALUES
(1, 'EHBO', 'Eerste hulp bij ongelukken', '2024-01-01', '2024-01-15', 10, NULL),
(2, 'Reisleiding', 'Training voor reisleiders', '2024-02-01', '2024-02-10', 5, 1),
(3, 'Groepsmanagement', 'Cursus voor groepsleiders', '2024-03-01', '2024-03-10', 8, 1),
(4, 'Avanced EHBO', 'Uitgebreide eerste hulp technieken', '2025-01-01', '2025-01-15', 10, 1),
(5, 'Culturele gids', 'Cursus over culturele gidsvaardigheden', '2025-02-01', '2025-02-10', 1, NULL),
(6, 'Eventplanning', 'Voorbereiding en organisatie van evenementen', '2025-03-01', '2025-03-10', 8, 2);
SET IDENTITY_INSERT Opleidingen OFF;

-- Gebruikers koppelen aan opleidingen
DELETE FROM OpleidingPersonen WHERE Id IN (1, 2, 3);
SET IDENTITY_INSERT OpleidingPersonen ON;
INSERT INTO OpleidingPersonen (Id, OpleidingId, PersoonId) VALUES
(1, 1, 1012), -- Monitor volgt EHBO
(2, 2, 1013), -- Hoofdmonitor volgt Reisleiding
(4, 4, 1012), -- Monitor volgt Advanced EHBOdate
(5, 5, 1013) -- Hoofdmonitor volgt Culturele gids
SET IDENTITY_INSERT OpleidingPersonen OFF;

-- Foto's toevoegen
DELETE FROM Fotos WHERE Id IN (1, 2, 3, 4);
SET IDENTITY_INSERT Fotos ON;
INSERT INTO Fotos (Id, Naam, BestemmingId) VALUES
(1, 'eiffeltoren.jpg', 1),
(2, 'londonbridge.jpg', 2),
(3, 'colosseum.jpg', 3),
(4, 'barcelona.jpg', 4);
SET IDENTITY_INSERT Fotos OFF;
