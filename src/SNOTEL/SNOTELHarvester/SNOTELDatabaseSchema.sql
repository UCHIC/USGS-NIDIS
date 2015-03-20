
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, and Azure
-- --------------------------------------------------
-- Date Created: 10/11/2014 15:42:17
-- Generated from EDMX file: E:\dev\github\USGS-NIDIS\src\SNOTEL\SNOTELHarvester\UpdateSnotel\SnoTelDataModel.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [SNOTELCatalog];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[FK_Variables_Units]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Variables] DROP CONSTRAINT [FK_Variables_Units];
GO
IF OBJECT_ID(N'[dbo].[FK_Variables_Units1]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Variables] DROP CONSTRAINT [FK_Variables_Units1];
GO

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[Methods]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Methods];
GO
IF OBJECT_ID(N'[dbo].[SeriesCatalog]', 'U') IS NOT NULL
    DROP TABLE [dbo].[SeriesCatalog];
GO
IF OBJECT_ID(N'[dbo].[Sites]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Sites];
GO
IF OBJECT_ID(N'[dbo].[Sources]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Sources];
GO
IF OBJECT_ID(N'[dbo].[Units]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Units];
GO
IF OBJECT_ID(N'[dbo].[Variables]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Variables];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'Methods'
CREATE TABLE [dbo].[Methods] (
    [MethodID] int  NOT NULL,
    [MethodDescription] nvarchar(max)  NOT NULL,
    [MethodLink] nvarchar(500)  NULL
);
GO

-- Creating table 'SeriesCatalog'
CREATE TABLE [dbo].[SeriesCatalog] (
    [SeriesID] int  NOT NULL,
    [SiteID] int  NULL,
    [SiteCode] nvarchar(50)  NULL,
    [SiteName] nvarchar(255)  NULL,
    [VariableID] int  NULL,
    [VariableCode] nvarchar(50)  NULL,
    [VariableName] nvarchar(255)  NULL,
    [Speciation] nvarchar(255)  NULL,
    [VariableUnitsID] int  NULL,
    [VariableUnitsName] nvarchar(255)  NULL,
    [SampleMedium] nvarchar(255)  NULL,
    [ValueType] nvarchar(255)  NULL,
    [TimeSupport] float  NULL,
    [TimeUnitsID] int  NULL,
    [TimeUnitsName] nvarchar(255)  NULL,
    [DataType] nvarchar(255)  NULL,
    [GeneralCategory] nvarchar(255)  NULL,
    [MethodID] int  NULL,
    [MethodDescription] nvarchar(max)  NULL,
    [SourceID] int  NULL,
    [Organization] nvarchar(255)  NULL,
    [SourceDescription] nvarchar(max)  NULL,
    [Citation] nvarchar(max)  NULL,
    [QualityControlLevelID] int  NULL,
    [QualityControlLevelCode] nvarchar(50)  NULL,
    [BeginDateTime] datetime  NULL,
    [EndDateTime] datetime  NULL,
    [BeginDateTimeUTC] datetime  NULL,
    [EndDateTimeUTC] datetime  NULL,
    [ValueCount] int  NULL
);
GO

-- Creating table 'Sites'
CREATE TABLE [dbo].[Sites] (
    [SiteID] int  NOT NULL,
    [SiteCode] nvarchar(50)  NOT NULL,
    [SiteName] nvarchar(255)  NOT NULL,
    [Latitude] float  NOT NULL,
    [Longitude] float  NOT NULL,
    [LatLongDatumID] int  NOT NULL,
    [Elevation_m] float  NULL,
    [VerticalDatum] nvarchar(255)  NULL,
    [LocalX] float  NULL,
    [LocalY] float  NULL,
    [LocalProjectionID] int  NULL,
    [PosAccuracy_m] float  NULL,
    [State] nvarchar(255)  NULL,
    [County] nvarchar(255)  NULL,
    [Comments] nvarchar(max)  NULL,
    [TimeZone] int  NULL,
    [Status] nvarchar(50)  NULL
);
GO

-- Creating table 'Sources'
CREATE TABLE [dbo].[Sources] (
    [SourceID] int  NOT NULL,
    [Organization] nvarchar(255)  NOT NULL,
    [SourceDescription] nvarchar(max)  NOT NULL,
    [SourceLink] nvarchar(500)  NULL,
    [ContactName] nvarchar(255)  NOT NULL,
    [Phone] nvarchar(255)  NOT NULL,
    [Email] nvarchar(255)  NOT NULL,
    [Address] nvarchar(255)  NOT NULL,
    [City] nvarchar(255)  NOT NULL,
    [State] nvarchar(255)  NOT NULL,
    [ZipCode] nvarchar(255)  NOT NULL,
    [Citation] nvarchar(max)  NOT NULL,
    [MetadataID] int  NOT NULL
);
GO

-- Creating table 'Units'
CREATE TABLE [dbo].[Units] (
    [UnitsID] int  NOT NULL,
    [UnitsName] nvarchar(255)  NOT NULL,
    [UnitsType] nvarchar(255)  NOT NULL,
    [UnitsAbbreviation] nvarchar(255)  NOT NULL
);
GO

-- Creating table 'Variables'
CREATE TABLE [dbo].[Variables] (
    [VariableID] int  NOT NULL,
    [VariableCode] nvarchar(50)  NOT NULL,
    [VariableName] nvarchar(255)  NOT NULL,
    [Speciation] nvarchar(255)  NOT NULL,
    [SampleMedium] nvarchar(255)  NOT NULL,
    [ValueType] nvarchar(255)  NOT NULL,
    [IsRegular] bit  NOT NULL,
    [TimeSupport] float  NOT NULL,
    [DataType] nvarchar(255)  NOT NULL,
    [GeneralCategory] nvarchar(255)  NOT NULL,
    [NoDataValue] float  NOT NULL,
    [Units_UnitsID] int  NOT NULL,
    [Units1_UnitsID] int  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [MethodID] in table 'Methods'
ALTER TABLE [dbo].[Methods]
ADD CONSTRAINT [PK_Methods]
    PRIMARY KEY CLUSTERED ([MethodID] ASC);
GO

-- Creating primary key on [SeriesID] in table 'SeriesCatalog'
ALTER TABLE [dbo].[SeriesCatalog]
ADD CONSTRAINT [PK_SeriesCatalog]
    PRIMARY KEY CLUSTERED ([SeriesID] ASC);
GO

-- Creating primary key on [SiteID] in table 'Sites'
ALTER TABLE [dbo].[Sites]
ADD CONSTRAINT [PK_Sites]
    PRIMARY KEY CLUSTERED ([SiteID] ASC);
GO

-- Creating primary key on [SourceID] in table 'Sources'
ALTER TABLE [dbo].[Sources]
ADD CONSTRAINT [PK_Sources]
    PRIMARY KEY CLUSTERED ([SourceID] ASC);
GO

-- Creating primary key on [UnitsID] in table 'Units'
ALTER TABLE [dbo].[Units]
ADD CONSTRAINT [PK_Units]
    PRIMARY KEY CLUSTERED ([UnitsID] ASC);
GO

-- Creating primary key on [VariableID] in table 'Variables'
ALTER TABLE [dbo].[Variables]
ADD CONSTRAINT [PK_Variables]
    PRIMARY KEY CLUSTERED ([VariableID] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [Units_UnitsID] in table 'Variables'
ALTER TABLE [dbo].[Variables]
ADD CONSTRAINT [FK_Variables_Units]
    FOREIGN KEY ([Units_UnitsID])
    REFERENCES [dbo].[Units]
        ([UnitsID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_Variables_Units'
CREATE INDEX [IX_FK_Variables_Units]
ON [dbo].[Variables]
    ([Units_UnitsID]);
GO

-- Creating foreign key on [Units1_UnitsID] in table 'Variables'
ALTER TABLE [dbo].[Variables]
ADD CONSTRAINT [FK_Variables_Units1]
    FOREIGN KEY ([Units1_UnitsID])
    REFERENCES [dbo].[Units]
        ([UnitsID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_Variables_Units1'
CREATE INDEX [IX_FK_Variables_Units1]
ON [dbo].[Variables]
    ([Units1_UnitsID]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------