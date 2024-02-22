#!/bin/bash

git remote remove upstream
git remote add upstream git@gitlab.gbar.dtu.dk:02141-s24/moral-template.git
git pull upstream main
