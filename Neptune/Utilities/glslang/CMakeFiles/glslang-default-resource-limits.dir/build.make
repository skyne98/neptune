# CMAKE generated file: DO NOT EDIT!
# Generated by "Unix Makefiles" Generator, CMake Version 3.9

# Delete rule output on recipe failure.
.DELETE_ON_ERROR:


#=============================================================================
# Special targets provided by cmake.

# Disable implicit rules so canonical targets will work.
.SUFFIXES:


# Remove some rules from gmake that .SUFFIXES does not remove.
SUFFIXES =

.SUFFIXES: .hpux_make_needs_suffix_list


# Suppress display of executed commands.
$(VERBOSE).SILENT:


# A target that is always out of date.
cmake_force:

.PHONY : cmake_force

#=============================================================================
# Set environment variables for the build.

# The shell in which to execute make rules.
SHELL = /bin/sh

# The CMake executable.
CMAKE_COMMAND = /usr/bin/cmake

# The command to remove a file.
RM = /usr/bin/cmake -E remove -f

# Escaping for special characters.
EQUALS = =

# The top-level source directory on which CMake was run.
CMAKE_SOURCE_DIR = /media/skyne/ExternalDrive/temp/SPIRV-Cross/external/glslang

# The top-level build directory on which CMake was run.
CMAKE_BINARY_DIR = /media/skyne/ExternalDrive/temp/SPIRV-Cross/external/glslang-build

# Include any dependencies generated for this target.
include StandAlone/CMakeFiles/glslang-default-resource-limits.dir/depend.make

# Include the progress variables for this target.
include StandAlone/CMakeFiles/glslang-default-resource-limits.dir/progress.make

# Include the compile flags for this target's objects.
include StandAlone/CMakeFiles/glslang-default-resource-limits.dir/flags.make

StandAlone/CMakeFiles/glslang-default-resource-limits.dir/ResourceLimits.cpp.o: StandAlone/CMakeFiles/glslang-default-resource-limits.dir/flags.make
StandAlone/CMakeFiles/glslang-default-resource-limits.dir/ResourceLimits.cpp.o: /media/skyne/ExternalDrive/temp/SPIRV-Cross/external/glslang/StandAlone/ResourceLimits.cpp
	@$(CMAKE_COMMAND) -E cmake_echo_color --switch=$(COLOR) --green --progress-dir=/media/skyne/ExternalDrive/temp/SPIRV-Cross/external/glslang-build/CMakeFiles --progress-num=$(CMAKE_PROGRESS_1) "Building CXX object StandAlone/CMakeFiles/glslang-default-resource-limits.dir/ResourceLimits.cpp.o"
	cd /media/skyne/ExternalDrive/temp/SPIRV-Cross/external/glslang-build/StandAlone && /usr/bin/c++  $(CXX_DEFINES) $(CXX_INCLUDES) $(CXX_FLAGS) -o CMakeFiles/glslang-default-resource-limits.dir/ResourceLimits.cpp.o -c /media/skyne/ExternalDrive/temp/SPIRV-Cross/external/glslang/StandAlone/ResourceLimits.cpp

StandAlone/CMakeFiles/glslang-default-resource-limits.dir/ResourceLimits.cpp.i: cmake_force
	@$(CMAKE_COMMAND) -E cmake_echo_color --switch=$(COLOR) --green "Preprocessing CXX source to CMakeFiles/glslang-default-resource-limits.dir/ResourceLimits.cpp.i"
	cd /media/skyne/ExternalDrive/temp/SPIRV-Cross/external/glslang-build/StandAlone && /usr/bin/c++ $(CXX_DEFINES) $(CXX_INCLUDES) $(CXX_FLAGS) -E /media/skyne/ExternalDrive/temp/SPIRV-Cross/external/glslang/StandAlone/ResourceLimits.cpp > CMakeFiles/glslang-default-resource-limits.dir/ResourceLimits.cpp.i

StandAlone/CMakeFiles/glslang-default-resource-limits.dir/ResourceLimits.cpp.s: cmake_force
	@$(CMAKE_COMMAND) -E cmake_echo_color --switch=$(COLOR) --green "Compiling CXX source to assembly CMakeFiles/glslang-default-resource-limits.dir/ResourceLimits.cpp.s"
	cd /media/skyne/ExternalDrive/temp/SPIRV-Cross/external/glslang-build/StandAlone && /usr/bin/c++ $(CXX_DEFINES) $(CXX_INCLUDES) $(CXX_FLAGS) -S /media/skyne/ExternalDrive/temp/SPIRV-Cross/external/glslang/StandAlone/ResourceLimits.cpp -o CMakeFiles/glslang-default-resource-limits.dir/ResourceLimits.cpp.s

StandAlone/CMakeFiles/glslang-default-resource-limits.dir/ResourceLimits.cpp.o.requires:

.PHONY : StandAlone/CMakeFiles/glslang-default-resource-limits.dir/ResourceLimits.cpp.o.requires

StandAlone/CMakeFiles/glslang-default-resource-limits.dir/ResourceLimits.cpp.o.provides: StandAlone/CMakeFiles/glslang-default-resource-limits.dir/ResourceLimits.cpp.o.requires
	$(MAKE) -f StandAlone/CMakeFiles/glslang-default-resource-limits.dir/build.make StandAlone/CMakeFiles/glslang-default-resource-limits.dir/ResourceLimits.cpp.o.provides.build
.PHONY : StandAlone/CMakeFiles/glslang-default-resource-limits.dir/ResourceLimits.cpp.o.provides

StandAlone/CMakeFiles/glslang-default-resource-limits.dir/ResourceLimits.cpp.o.provides.build: StandAlone/CMakeFiles/glslang-default-resource-limits.dir/ResourceLimits.cpp.o


# Object files for target glslang-default-resource-limits
glslang__default__resource__limits_OBJECTS = \
"CMakeFiles/glslang-default-resource-limits.dir/ResourceLimits.cpp.o"

# External object files for target glslang-default-resource-limits
glslang__default__resource__limits_EXTERNAL_OBJECTS =

StandAlone/libglslang-default-resource-limits.a: StandAlone/CMakeFiles/glslang-default-resource-limits.dir/ResourceLimits.cpp.o
StandAlone/libglslang-default-resource-limits.a: StandAlone/CMakeFiles/glslang-default-resource-limits.dir/build.make
StandAlone/libglslang-default-resource-limits.a: StandAlone/CMakeFiles/glslang-default-resource-limits.dir/link.txt
	@$(CMAKE_COMMAND) -E cmake_echo_color --switch=$(COLOR) --green --bold --progress-dir=/media/skyne/ExternalDrive/temp/SPIRV-Cross/external/glslang-build/CMakeFiles --progress-num=$(CMAKE_PROGRESS_2) "Linking CXX static library libglslang-default-resource-limits.a"
	cd /media/skyne/ExternalDrive/temp/SPIRV-Cross/external/glslang-build/StandAlone && $(CMAKE_COMMAND) -P CMakeFiles/glslang-default-resource-limits.dir/cmake_clean_target.cmake
	cd /media/skyne/ExternalDrive/temp/SPIRV-Cross/external/glslang-build/StandAlone && $(CMAKE_COMMAND) -E cmake_link_script CMakeFiles/glslang-default-resource-limits.dir/link.txt --verbose=$(VERBOSE)

# Rule to build all files generated by this target.
StandAlone/CMakeFiles/glslang-default-resource-limits.dir/build: StandAlone/libglslang-default-resource-limits.a

.PHONY : StandAlone/CMakeFiles/glslang-default-resource-limits.dir/build

StandAlone/CMakeFiles/glslang-default-resource-limits.dir/requires: StandAlone/CMakeFiles/glslang-default-resource-limits.dir/ResourceLimits.cpp.o.requires

.PHONY : StandAlone/CMakeFiles/glslang-default-resource-limits.dir/requires

StandAlone/CMakeFiles/glslang-default-resource-limits.dir/clean:
	cd /media/skyne/ExternalDrive/temp/SPIRV-Cross/external/glslang-build/StandAlone && $(CMAKE_COMMAND) -P CMakeFiles/glslang-default-resource-limits.dir/cmake_clean.cmake
.PHONY : StandAlone/CMakeFiles/glslang-default-resource-limits.dir/clean

StandAlone/CMakeFiles/glslang-default-resource-limits.dir/depend:
	cd /media/skyne/ExternalDrive/temp/SPIRV-Cross/external/glslang-build && $(CMAKE_COMMAND) -E cmake_depends "Unix Makefiles" /media/skyne/ExternalDrive/temp/SPIRV-Cross/external/glslang /media/skyne/ExternalDrive/temp/SPIRV-Cross/external/glslang/StandAlone /media/skyne/ExternalDrive/temp/SPIRV-Cross/external/glslang-build /media/skyne/ExternalDrive/temp/SPIRV-Cross/external/glslang-build/StandAlone /media/skyne/ExternalDrive/temp/SPIRV-Cross/external/glslang-build/StandAlone/CMakeFiles/glslang-default-resource-limits.dir/DependInfo.cmake --color=$(COLOR)
.PHONY : StandAlone/CMakeFiles/glslang-default-resource-limits.dir/depend

