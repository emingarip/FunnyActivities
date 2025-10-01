-- Migration: AddSurveySystemEntities
-- Creates Survey system tables

-- Create Surveys table
CREATE TABLE "Surveys" (
    "Id" uuid NOT NULL,
    "Title" character varying(200) NOT NULL,
    "Description" character varying(1000),
    "CreatedByUserId" uuid NOT NULL,
    "IsActive" boolean NOT NULL DEFAULT true,
    "StartDate" timestamp with time zone NOT NULL,
    "EndDate" timestamp with time zone,
    "MaxParticipants" integer,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Surveys" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Surveys_Users_CreatedByUserId" FOREIGN KEY ("CreatedByUserId") REFERENCES "Users"("Id") ON DELETE RESTRICT
);

-- Create SurveyActivities table
CREATE TABLE "SurveyActivities" (
    "Id" uuid NOT NULL,
    "SurveyId" uuid NOT NULL,
    "ActivityId" uuid NOT NULL,
    "Order" integer NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_SurveyActivities" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_SurveyActivities_Activities_ActivityId" FOREIGN KEY ("ActivityId") REFERENCES "Activities"("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_SurveyActivities_Surveys_SurveyId" FOREIGN KEY ("SurveyId") REFERENCES "Surveys"("Id") ON DELETE CASCADE
);

-- Create SurveyParticipants table
CREATE TABLE "SurveyParticipants" (
    "Id" uuid NOT NULL,
    "SurveyId" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "ParticipatedAt" timestamp with time zone NOT NULL,
    "CompletedAt" timestamp with time zone,
    "IsCompleted" boolean NOT NULL DEFAULT false,
    CONSTRAINT "PK_SurveyParticipants" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_SurveyParticipants_Surveys_SurveyId" FOREIGN KEY ("SurveyId") REFERENCES "Surveys"("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_SurveyParticipants_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users"("Id") ON DELETE CASCADE
);

-- Create SurveyVotes table
CREATE TABLE "SurveyVotes" (
    "Id" uuid NOT NULL,
    "SurveyId" uuid NOT NULL,
    "SurveyActivityId" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "VoteValue" integer NOT NULL,
    "Comment" character varying(500),
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_SurveyVotes" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_SurveyVotes_SurveyActivities_SurveyActivityId" FOREIGN KEY ("SurveyActivityId") REFERENCES "SurveyActivities"("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_SurveyVotes_Surveys_SurveyId" FOREIGN KEY ("SurveyId") REFERENCES "Surveys"("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_SurveyVotes_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users"("Id") ON DELETE CASCADE,
    CONSTRAINT "CK_SurveyVotes_VoteValue_Range" CHECK ("VoteValue" >= 1 AND "VoteValue" <= 5)
);

-- Create indexes for better performance
CREATE INDEX "IX_Surveys_CreatedByUserId" ON "Surveys" ("CreatedByUserId");
CREATE INDEX "IX_Surveys_IsActive" ON "Surveys" ("IsActive");
CREATE INDEX "IX_Surveys_StartDate" ON "Surveys" ("StartDate");
CREATE UNIQUE INDEX "IX_Surveys_Title" ON "Surveys" ("Title");

CREATE INDEX "IX_SurveyActivities_ActivityId" ON "SurveyActivities" ("ActivityId");
CREATE INDEX "IX_SurveyActivities_SurveyId" ON "SurveyActivities" ("SurveyId");
CREATE UNIQUE INDEX "IX_SurveyActivities_SurveyId_Order" ON "SurveyActivities" ("SurveyId", "Order");

CREATE INDEX "IX_SurveyParticipants_SurveyId" ON "SurveyParticipants" ("SurveyId");
CREATE INDEX "IX_SurveyParticipants_UserId" ON "SurveyParticipants" ("UserId");
CREATE UNIQUE INDEX "IX_SurveyParticipants_SurveyId_UserId" ON "SurveyParticipants" ("SurveyId", "UserId");

CREATE INDEX "IX_SurveyVotes_SurveyActivityId" ON "SurveyVotes" ("SurveyActivityId");
CREATE INDEX "IX_SurveyVotes_SurveyId" ON "SurveyVotes" ("SurveyId");
CREATE INDEX "IX_SurveyVotes_UserId" ON "SurveyVotes" ("UserId");
CREATE UNIQUE INDEX "IX_SurveyVotes_SurveyId_SurveyActivityId_UserId" ON "SurveyVotes" ("SurveyId", "SurveyActivityId", "UserId");

-- Insert migration record into __EFMigrationsHistory
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250924150000_AddSurveySystemEntities', '8.0.8');