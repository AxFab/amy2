#pragma once
#include <iostream>
#include <fstream>
#include <map>
#include <string>
#include "UnitBuilder.h"

class UnitParser
{
private:
    std::ifstream rd;
    UnitBuilder builder;
    std::map<std::string, LogicalVector> vectors;
    std::map<std::string, int> constantes;
public:
    UnitParser(const std::string &path);
    LogicalVector parseLoop(int loop, const std::string &name, const std::string &name2);
    LogicalVector parseSelect(LogicalVector vc);
    LogicalVector parseLoopEntry(std::string &ln, const std::string &name);
    LogicalVector parseSelectEntry(std::string &ln);
    LogicalVector parseVecEntry(std::string &ln, GateOpcode op);
    LogicalVector readVectors(std::string &ln);
    LogicalVector readVector(const std::string &wrd, std::string &ln);
    LogicalVector parseStatement(std::string &ln, const std::string &name);

    int readNumber(std::string &str);
    void parseBlock(const std::string &name);

    void parseLine(std::string &ln);
    std::string nextLine();
    void parse();
};
