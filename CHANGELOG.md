# Changelog

## [1.0.0](https://github.com/rosslight/Darp.BinaryObjects/compare/v0.7.0...v1.0.0) (2026-10-01)


### ⚠ BREAKING CHANGES

* modernize releases and .NET targets ([#2](https://github.com/rosslight/Darp.BinaryObjects/issues/2))

### Bug Fixes

* **generator:** reject counts below collection minimum ([#14](https://github.com/rosslight/Darp.BinaryObjects/issues/14)) ([1dc6f3c](https://github.com/rosslight/Darp.BinaryObjects/commit/1dc6f3cfed492252dd4e2b6c9e21c55017d586bf))
* **generator:** support narrow minimum-count fields ([#15](https://github.com/rosslight/Darp.BinaryObjects/issues/15)) ([76eb454](https://github.com/rosslight/Darp.BinaryObjects/commit/76eb4549e4cb5a7f0f8a38a79b17a5fccbe16b9a))
* **generator:** warn when binary object class has a base class ([#13](https://github.com/rosslight/Darp.BinaryObjects/issues/13)) ([af7c04d](https://github.com/rosslight/Darp.BinaryObjects/commit/af7c04d2a50c9150dd18e794aaccd2fb71c9a3df))
* guard collection lengths and require remaining collections last ([#9](https://github.com/rosslight/Darp.BinaryObjects/issues/9)) ([e86f9b9](https://github.com/rosslight/Darp.BinaryObjects/commit/e86f9b93ac03369f9799af295f703d97c1f1fbcc))
* handle object collection counts correctly ([#7](https://github.com/rosslight/Darp.BinaryObjects/issues/7)) ([1947104](https://github.com/rosslight/Darp.BinaryObjects/commit/194710499240fcdf685c2ac2d845459edee07de1))
* honor binary object generation options ([#8](https://github.com/rosslight/Darp.BinaryObjects/issues/8)) ([cfd049f](https://github.com/rosslight/Darp.BinaryObjects/commit/cfd049f5d6f43d3c5b31c5c1415335e2c5b9b9cf))
* honor constructor parameter order in generated readers ([#4](https://github.com/rosslight/Darp.BinaryObjects/issues/4)) ([cc15ce5](https://github.com/rosslight/Darp.BinaryObjects/commit/cc15ce57829e02c159143ed6124b294ffa853617))
* infer nested sizes from serialized members ([#5](https://github.com/rosslight/Darp.BinaryObjects/issues/5)) ([1cdc636](https://github.com/rosslight/Darp.BinaryObjects/commit/1cdc636eeb119b5915fe72ca25911cffbfe08506))
* propagate fixed-size child serializer failures ([#12](https://github.com/rosslight/Darp.BinaryObjects/issues/12)) ([d81c87a](https://github.com/rosslight/Darp.BinaryObjects/commit/d81c87a527c1b511be75179d44d96aa14c42c0ec))
* use manual serializers' reported byte counts ([#10](https://github.com/rosslight/Darp.BinaryObjects/issues/10)) ([fabb257](https://github.com/rosslight/Darp.BinaryObjects/commit/fabb2577d522b8e8cd2e6a63ae2333dd646b43bc))


### Build System

* modernize releases and .NET targets ([#2](https://github.com/rosslight/Darp.BinaryObjects/issues/2)) ([7b54816](https://github.com/rosslight/Darp.BinaryObjects/commit/7b548167bfa6289eda8fa05be92f811c509fe198))
