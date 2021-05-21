using System;
using System.Collections.Generic;
using System.Text;
using GetJobProfiles.Entities;
using GetJobProfiles.Models.Recipe.ContentItems.EntryRoutes;

namespace GetJobProfiles.Models.Stax
{
    public class StaxJobProfileeModel
    {
        // As of 22/4/21 the JobProfile content type consists of the following content types
        // Level 1                                                                                                                                      JobCategory
        //                                                                                                                                                   |
        // Level 2                                                                                                                                       JobProfile
        //                                                                                                                                                   |
        //             +-------------+-----------------+----------------------+-------------------------+-------------+-------------+--------------+---------+-------+-----------------+-------------+-------------+-------------+-------------+-------------+-------------+-------------+
        //             |             |                 |                      |                         |             |             |              |                 |                 |             |             |             |             |             |             |             |
        // Level 3  SOCCode        ONet           University               College                Apprenticeship    Work      Volunteering     DirectRoute         Other          Registration   Restriction     Other         Digital       Working       Working       Working   Apprenticeship
        //                     Occupational          Route                  Route                     Route         Route         Route          Route             Route                                      Requirement      Skills       Environment    Location      Uniform      Standard
        //                        Code                |                      |                         |                                                   (SpecialistTraining)                                                Level                                                    |
        //                          |           +-----+-----+          +-----+-----+            +------+------+                                                                                                                                                                   +-----+-----+
        //                          |           |           |          |           |            |             |                                                                                                                                                                   |           |
        // Level 4              ONetSkill   University  University   College      College  Apprenticeship  Apprenticeship                                                                                                                                                    Apprenticeship  QCFLevel
        //                                  Requirement    Link    Requirement     Link     Requirement         Link                                                                                                                                                           Standard
        //                                                                                                                                                                                                                                                                      Route
        // Need to be mindful of the dependencies and generate the recipe files from the bottom up

        // Reference data public properties
        public OnetSkill OnetSkill { get; private set; }

        // Job Profile public properties
        public UniversityRequirementContentItem UniversityRequirementContentItem { get; private set; }


    }
}
