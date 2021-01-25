# Orchard Core Dashboard Progress

Orchard Core’s new Dashboard module seems (MVP) feature complete - the implementing developer has RFC’ed.

It supports the creation of widgets that get displayed in the main window of the admin area.

Using it would free us from implementing infrastructure code related to display, permissions, etc. (Note: when I tried accessing the dashboard using an user with author role, with seemingly the right permissions, the dashboard wasn’t displayed due to a permissions issue).

The initial version doesn’t contain features that we’d find useful, such as displaying widgets according to user role.

The feature is not self contained in a module. (`Drivers/HtmlBodyPartDisplayDriver.cs` & `MarkdownBodyPartDisplayDriver.cs` have been updated). This means we can’t bring the source in the PR into the Stax Editor. Instead we could incorporate the changes into the version of Orchard Core we package up. Unless we don’t need to support dashboard widget constructed using HtmlBody and MarkdownBody (using only custom parts), in which case, we could just bring over the source code for the module (OrchardCore.AdminDashboard).

## Proof of concept

This branch contains a proof of concept dashboard, with the current GraphSyncPart displaying markup into a dashboard widget.

The Orchard Core packages have been built from the Dashboard PR. The corresponding issue contains some useful info about the implementation.

To test the dashboard:

* Enable the Admin Dashboard feature.

* Create a content type with `DashboardWidget` Stereotype and add Dashboard Part and Graph Sync Part.

* Add widget of the created type to the dashboard.

Now when you view the dashboard, you should see a widget containing the DisplayAdmin shape of the GraphSync part (`GraphSyncPart_DetailAdmin.cshtml`).

Implementing a widget this way would mean querying in the display driver to populate the widget’s view model.

The Orchard Core developers suggested this implementation:

> If it can be queried in the db, the Liquid part and a custom query that retrieves the information you want is the easiest since it requires no code.

However, it might be better to stick to a razor view, as it gives us more flexibility and doesn't require the developer to know/learn the liquid syntax (also DetailAdmin support doesn't seem to have been added to the liquid part yet).

## Potential Implementation

Create an authorisation workflow module.

Create an authorisation dashboard custom part. Part will have settings with a select to indicate which type of dashboard it is. Part driver will fetch global/role/user data, populate view model and present view according to type in settings.

As role/user support hasn’t been baked into the dashboard infrastructure yet, it may mean some roles see dashboard items that we’ll have to populate with a message such as ‘you do not have the ability to xxx’.

## Resources

[Dassboard Video](https://www.youtube.com/watch?v=MQuiXEnyEBw&utm_source=Lombiq%27s+Orchard+Dojo+Newsletter&utm_campaign=f1e976ed55-EMAIL_CAMPAIGN_2019_12_22_06_53_COPY_07&utm_medium=email&utm_term=0_039db8f13f-f1e976ed55-373788729)

An [older dashboard video](https://www.youtube.com/watch?v=y6OSdfSwmnY&t=0s) (not up-to-date, but interesting).

[Dashboard Blog Post](https://orcharddojo.net/blog/admin-dashboard-display-titles-in-top-bar-this-week-in-orchard-24-12-2020?utm_source=Lombiq%27s+Orchard+Dojo+Newsletter&utm_campaign=f1e976ed55-EMAIL_CAMPAIGN_2019_12_22_06_53_COPY_07&utm_medium=email&utm_term=0_039db8f13f-f1e976ed55-373788729)

