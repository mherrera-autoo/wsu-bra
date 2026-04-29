namespace ERP.Modules.Rfid.Domain;

public enum RfidTagStatus
{
    Active = 1,
    Retired = 2
}

public enum RfidEventType
{
    PortalExit = 1,
    AccessEntry = 2,
    AccessExit = 3,
    HandheldScan = 4
}

public enum RfidInboxProcessingStatus
{
    Pending = 1,
    Processed = 2,
    Failed = 3
}

public enum RfidAccessSessionStatus
{
    Active = 1,
    Closed = 2,
    Abandoned = 3
}
