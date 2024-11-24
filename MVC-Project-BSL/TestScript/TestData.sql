-- Kinderen toevoegen
DELETE FROM Kinderen WHERE PersoonId IN (1, 2);
SET IDENTITY_INSERT Kinderen ON;
INSERT INTO Kinderen (Id, PersoonId, Voornaam, Naam, Geboortedatum, Allergieen, Medicatie) VALUES
(1, 1, 'Emma', 'Doe', '2010-06-01', 'Noten', 'Epipen'),
(2, 1, 'Tom', 'Doe', '2012-12-15', 'Geen', 'Geen');
SET IDENTITY_INSERT Kinderen OFF;

-- Bestemmingen
DELETE FROM Bestemmingen WHERE Id IN (1, 2);
SET IDENTITY_INSERT Bestemmingen ON;
INSERT INTO Bestemmingen (Id, Code, BestemmingsNaam, Beschrijving, MinLeeftijd, MaxLeeftijd) VALUES
(1, 'B001', 'Parijs', 'Een reis naar Parijs.', 10, 18),
(2, 'B002', 'Londen', 'Een culturele trip naar Londen.', 12, 18);
SET IDENTITY_INSERT Bestemmingen OFF;

-- Groepsreizen toevoegen
DELETE FROM Groepsreizen WHERE Id IN (1, 2);
SET IDENTITY_INSERT Groepsreizen ON;
INSERT INTO Groepsreizen (Id, BestemmingId, BeginDatum, EindDatum, Prijs, IsArchived) VALUES
(1, 1, '2024-05-01', '2024-05-07', 500.00, 1),
(2, 2, '2024-06-10', '2024-06-15', 600.00, 0);
SET IDENTITY_INSERT Groepsreizen OFF;

-- Activiteiten toevoegen
DELETE FROM Activiteiten WHERE Id IN (1, 2, 3);
SET IDENTITY_INSERT Activiteiten ON;
INSERT INTO Activiteiten (Id, Naam, Beschrijving) VALUES
(1, 'Eiffeltoren Bezoek', 'Een bezoek aan de Eiffeltoren.'),
(2, 'Musea Bezoeken', 'Ontdek de rijke geschiedenis van Parijs.'),
(3, 'Londen Brug', 'Bezoek de iconische brug van Londen.');
SET IDENTITY_INSERT Activiteiten OFF;

-- Programma's koppelen aan groepsreizen
DELETE FROM Programmas WHERE GroepsreisId IN (1, 2);
INSERT INTO Programmas (Id, ActiviteitId, GroepsreisId) VALUES
(1, 1, 1), -- Activiteit 1 bij Parijs
(2, 2, 1), -- Activiteit 2 bij Parijs
(3, 3, 2); -- Activiteit 3 bij Londen

-- Foto's toevoegen
DELETE FROM Fotos WHERE Id IN (1, 2);
SET IDENTITY_INSERT Fotos ON;
INSERT INTO Fotos (Id, Naam, BestemmingId) VALUES
(1, 'eiffeltoren.jpg', 1),
(2, 'londonbridge.jpg', 2);
SET IDENTITY_INSERT Fotos OFF;

-- Onkosten toevoegen
DELETE FROM Onkosten WHERE Id IN (1, 2);
SET IDENTITY_INSERT Onkosten ON;
INSERT INTO Onkosten (Id, Titel, Omschrijving, Bedrag, Datum, Foto, GroepsreisId) VALUES
(1, 'Lunch in Parijs', 'Groepslunch tijdens reis.', 150.00, '2024-05-03', 'lunch.jpg', 1),
(2, 'Treintickets', 'Reis naar Londen.', 300.00, '2024-06-11', NULL, 2);
SET IDENTITY_INSERT Onkosten OFF;


-- Monitoren koppelen aan groepsreizen
DELETE FROM GroepsreisMonitor WHERE GroepsreisId IN (1, 2);
INSERT INTO GroepsreisMonitor (GroepsreisId, MonitorId) VALUES
(1, 1), -- Monitor bij Parijs
(2, 2); -- Hoofdmonitor bij Londen

-- Deelnemers toevoegen
DELETE FROM Deelnemers WHERE Id IN (1, 2);
SET IDENTITY_INSERT Deelnemers ON;
INSERT INTO Deelnemers (Id, KindId, GroepsreisDetailId, Opmerkingen, ReviewScore, Review) VALUES
(1, 1, 1, 'Geweldige reis!', 5, 'Fantastische ervaring.'),
(2, 2, 2, 'Leuke reis!', 4, 'Goed georganiseerd.');
SET IDENTITY_INSERT Deelnemers OFF;

-- Opleidingen toevoegen
DELETE FROM Opleidingen WHERE Id IN (1, 2);
SET IDENTITY_INSERT Opleidingen ON;
INSERT INTO Opleidingen (Id, Naam, Beschrijving, BeginDatum, EindDatum, AantalPlaatsen, OpleidingVereist) VALUES
(1, 'EHBO', 'Eerste hulp bij ongelukken', '2024-01-01', '2024-01-15', 10, NULL),
(2, 'Reisleiding', 'Training voor reisleiders', '2024-02-01', '2024-02-10', 5, 1);
SET IDENTITY_INSERT Opleidingen OFF;

-- Gebruikers koppelen aan opleidingen
DELETE FROM OpleidingPersonen WHERE Id IN (1, 2);
INSERT INTO OpleidingPersonen (Id, OpleidingId, PersoonId) VALUES
(1, 1, 2), -- Monitor volgt EHBO
(2, 2, 3); -- Hoofdmonitor volgt Reisleiding

-- Onkosten registreren
DELETE FROM Onkosten WHERE Id IN (3, 4);
SET IDENTITY_INSERT Onkosten ON;
INSERT INTO Onkosten (Id, Titel, Omschrijving, Bedrag, Datum, Foto, GroepsreisId) VALUES
(3, 'Extra drankjes', 'Drankjes tijdens diner', 50.00, '2024-05-04', NULL, 1),
(4, 'Souvenirs', 'Kleine cadeaus', 30.00, '2024-06-12', NULL, 2);
SET IDENTITY_INSERT Onkosten OFF;
