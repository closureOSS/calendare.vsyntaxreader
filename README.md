# Library to parse and manipulate ICS files based on RFC5545

[![🏭 Build](https://github.com/closureOSS/calendare.vsyntaxreader/actions/workflows/dotnet.yml/badge.svg)](https://github.com/closureOSS/calendare.vsyntaxreader/actions/workflows/dotnet.yml)

# Why

This C# library was specifically developed for reading and manipulating RFC5545 iCalendar (ICS) files, with a primary focus on **server-side applications**, such as calendar servers.  The goal was to create a robust and permissive library that can handle the many quirks and non-compliant behaviors of various calendar clients in the wild.

* Permissive Parsing: The library is built to be tolerant of malformed or non-standard ICS data, allowing it to successfully parse files from as many different clients as possible.

* Timezone Handling: Accurate handling of timezones is crucial for international scheduling. This library includes a dedicated and robust system for processing timezone definitions to ensure events are displayed correctly for users across the globe.

* Modern Feature Support: This includes support for the components VAvailability (RFC 7953), which is helpful for free/busy scheduling, and VPoll (RFC 9073), which enables calendar-based polling and voting features.

* This library is not intended for client-side use, such as creating new events or managing a user's local calendar.

# Install

Installation is simple, just install via

~~~shell
dotnet add package ClosureOSS.Calendare.VSyntaxReader
~~~

# Usage

Refer to the examples in the VSyntaxReader.Examples folder.

# Scope

## Supported RFC's

- [Internet Calendaring and Scheduling Core Object Specification (iCalendar)](https://datatracker.ietf.org/doc/html/rfc5545)
- [Parameter Value Encoding in iCalendar and vCard](https://datatracker.ietf.org/doc/html/rfc6868)
- [Scheduling Extensions to CalDAV](https://datatracker.ietf.org/doc/html/rfc6638)

## Partial supported RFC's

- [Calendar Availability](https://datatracker.ietf.org/doc/html/rfc7953) defines VAVAILABILITY component
- [Event Publishing Extensions to iCalendar](https://datatracker.ietf.org/doc/html/rfc9073) for the participant, location and resource component
- [New Properties for iCalendar](https://datatracker.ietf.org/doc/html/rfc7986)

## RFC's under consideration

- currently none

## Not supported RFC's

- [Non-Gregorian Recurrence Rules in the Internet Calendaring and Scheduling Core Object Specification (iCalendar)](https://datatracker.ietf.org/doc/html/rfc7529) extends RRULE with RSCALE and SKIP



# Credits

## Testcases (ICS files)

The test ICS files, in the ./data/ subdirectories, are largely derived from [Ical.Net](https://github.com/ical-org/ical.net) version 4 (circa February 2023). Ical.Net's test cases were originally created for the [libical project](https://github.com/libical/libical).

Some test ICS files where originally created for [DAViCal](https://gitlab.com/davical-project/davical).

These files may have been altered after they were copied from Ical.Net or DAViCal.
