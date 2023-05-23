using System;

namespace RPBlurb
{
  public interface ICharacterRoleplayData : IDisposable
  {
    public string? World { get; set; }
    public string? User { get; set; }
    public string? Name { get; set; }
    public int? NameStyle { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Alignment { get; set; }
    public string? Status { get; set; }
    public bool Loading { get; set; }
    public bool Invalid { get; set; }
    public bool Modified { get; set; }
  }
}
