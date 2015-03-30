USE [MingleAnalyticsWeb]
GO

/****** Object:  Table [dbo].[ContentUpdateQueue]    Script Date: 1/6/2015 12:47:57 PM ******/
SET ANSI_NULLS OFF
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ContentUpdateQueue](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[TableAltered] [nvarchar](100) NULL,
	[ActionPerformed] [nvarchar](50) NULL,
	[RecordIdentifier] [int] NULL,
	[SubYear] [nvarchar](4) NULL,
	[ClientId] [int] NULL,
	[PracticeId] [int] NULL,
	[ProviderId] [int] NULL,
	[SubmissionMethod] [nvarchar](10) NULL,
	[GProRegId] [nvarchar](255) NULL,
	[Submitting] [nvarchar](1) NULL,
	[HasConsent] [nvarchar](1) NULL,
	[ConsentBy] [nvarchar](255) NULL,
	[Submitted] [nvarchar](1) NULL,
	[LastGeneratedDate] [datetime] NULL,
	[MedicareBatchId] [nvarchar](50) NULL,
	[Step1Value] [tinyint] NULL,
	[Step2Value] [tinyint] NULL,
	[Step3Value] [tinyint] NULL,
	[Step4Value] [tinyint] NULL,
	[Step5Value] [tinyint] NULL,
	[Step6Value] [tinyint] NULL,
	[Step7Value] [tinyint] NULL,
	[Step8Value] [tinyint] NULL,
	[Step9Value] [tinyint] NULL,
	[Step10Value] [tinyint] NULL,
	[DateTimeAddedToQueue] [smalldatetime] NULL,
	[IsDeliveredToSalesForce] [bit] NULL CONSTRAINT [DF_ContentUpdateQueue_IsDeliveredToSalesForce]  DEFAULT ((0)),
	[DateTimeDeliveredToSalesForce] [smalldatetime] NULL,
	[ResultMessage] [nvarchar](max) NULL,
 CONSTRAINT [PK_ContentUpdateQueue] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO


