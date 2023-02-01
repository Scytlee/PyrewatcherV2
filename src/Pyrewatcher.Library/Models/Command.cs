﻿using Pyrewatcher.Library.Enums;

namespace Pyrewatcher.Library.Models;

public class Command
{
  public long Id { get; set; }
  public long? ParentId { get; set; }
  public string Keyword { get; set; }
  public string? Subkeyword { get; set; }
  public int CooldownInSeconds { get; set; }
  public int Permissions { get; set; }
  public CommandType Type { get; set; }
  public DateTime? LatestExecutionUtc { get; set; }
  public string? CustomText { get; set; }
}