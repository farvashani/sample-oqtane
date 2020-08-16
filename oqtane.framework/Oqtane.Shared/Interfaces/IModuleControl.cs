﻿using Oqtane.Models;
using Oqtane.Shared;
using System.Collections.Generic;

namespace Oqtane.Modules
{
    public interface IModuleControl
    {
        SecurityAccessLevel SecurityAccessLevel { get; } // defines the security access level for this control - defaults to View
        string Title { get; } // title to display for this control - defaults to module title
        string Actions { get; } // allows for routing by configuration rather than by convention ( comma delimited ) - defaults to using component file name
        bool UseAdminContainer { get; } // container for embedding module control - defaults to true. false will suppress the default modal UI popup behavior and render the component in the page.
        List<Resource> Resources { get; } // identifies all resources in a module
    }
}
